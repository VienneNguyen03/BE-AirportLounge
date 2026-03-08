using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record ReviewLeaveRequestCommand(
    Guid LeaveRequestId,
    bool Approve,
    string? Comment) : IRequest<Result<bool>>;

public class ReviewLeaveRequestCommandHandler : IRequestHandler<ReviewLeaveRequestCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;

    public ReviewLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ICacheService cache,
        INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(ReviewLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<bool>.Failure("Only administrators and managers can review leave requests");

        var leaveRequest = await _unitOfWork.LeaveRequests.Query()
            .Include(lr => lr.Employee).ThenInclude(e => e.User)
            .Include(lr => lr.LeaveType)
            .FirstOrDefaultAsync(lr => lr.Id == request.LeaveRequestId && !lr.IsDeleted, cancellationToken);

        if (leaveRequest is null)
            return Result<bool>.Failure("Leave request not found");

        if (leaveRequest.Status != LeaveRequestStatus.Pending)
            return Result<bool>.Failure($"Leave request has already been {leaveRequest.Status.ToString().ToLowerInvariant()}");

        if (request.Approve)
        {
            var balance = await _unitOfWork.LeaveBalances.Query()
                .FirstOrDefaultAsync(lb =>
                    lb.EmployeeId == leaveRequest.EmployeeId &&
                    lb.LeaveTypeId == leaveRequest.LeaveTypeId &&
                    lb.Year == leaveRequest.StartDate.Year &&
                    !lb.IsDeleted, cancellationToken);

            if (balance is null)
                return Result<bool>.Failure("Leave balance not found for this employee");

            if (balance.RemainingDays < leaveRequest.TotalDays)
                return Result<bool>.Failure(
                    $"Insufficient leave balance. Available: {balance.RemainingDays} days, Requested: {leaveRequest.TotalDays} days");

            balance.UsedDays += leaveRequest.TotalDays;
            balance.UpdatedBy = _currentUser.Email;
            _unitOfWork.LeaveBalances.Update(balance);

            leaveRequest.Status = LeaveRequestStatus.Approved;
        }
        else
        {
            leaveRequest.Status = LeaveRequestStatus.Rejected;
        }

        leaveRequest.ReviewedById = _currentUser.UserId;
        leaveRequest.ReviewedAt = DateTime.UtcNow;
        leaveRequest.ReviewerComment = request.Comment;
        leaveRequest.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveRequests.Update(leaveRequest);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.LeaveRequests(leaveRequest.EmployeeId), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.LeaveRequestsPending, cancellationToken);
        await _cache.RemoveAsync(
            CacheKeys.LeaveBalance(leaveRequest.EmployeeId, leaveRequest.StartDate.Year), cancellationToken);

        var statusText = request.Approve ? "approved" : "rejected";
        await _notifications.SendToUserAsync(
            leaveRequest.Employee.UserId,
            $"Leave Request {(request.Approve ? "Approved" : "Rejected")}",
            $"Your {leaveRequest.LeaveType.Name} request for {leaveRequest.StartDate:yyyy-MM-dd} to {leaveRequest.EndDate:yyyy-MM-dd} has been {statusText}."
                + (string.IsNullOrWhiteSpace(request.Comment) ? "" : $" Comment: {request.Comment}"),
            cancellationToken);

        return Result<bool>.Success(true, $"Leave request {statusText} successfully");
    }
}
