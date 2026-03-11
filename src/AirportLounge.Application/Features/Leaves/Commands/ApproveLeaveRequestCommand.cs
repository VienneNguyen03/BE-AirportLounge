using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Approve: UnderReview → Approved (Manager) ────────────────────
public record ApproveLeaveRequestCommand(
    Guid LeaveRequestId,
    string? Comment) : IRequest<Result<bool>>;

public class ApproveLeaveRequestCommandHandler : IRequestHandler<ApproveLeaveRequestCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;

    public ApproveLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICacheService cache, INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(ApproveLeaveRequestCommand request, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<bool>.Failure("Only managers can approve leave requests");

        var lr = await _unitOfWork.LeaveRequests.Query()
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == request.LeaveRequestId && !x.IsDeleted, ct);

        if (lr is null)
            return Result<bool>.Failure("Leave request not found");

        // Idempotent
        if (lr.Status == LeaveRequestStatus.Approved)
            return Result<bool>.Success(true, "Already approved");

        if (lr.Status != LeaveRequestStatus.UnderReview)
            return Result<bool>.Failure($"Cannot approve: status is '{lr.Status}'");

        // Guard: check balance
        var balance = await _unitOfWork.LeaveBalances.Query()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == lr.EmployeeId &&
                lb.LeaveTypeId == lr.LeaveTypeId &&
                lb.Year == lr.StartDate.Year &&
                !lb.IsDeleted, ct);

        if (balance is null)
            return Result<bool>.Failure("Leave balance not found for this employee");

        if (balance.RemainingDays < lr.TotalDays)
            return Result<bool>.Failure(
                $"Insufficient leave balance. Available: {balance.RemainingDays} days, Requested: {lr.TotalDays} days");

        // Side effect: reserve balance
        balance.ReservedDays += lr.TotalDays;
        balance.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveBalances.Update(balance);

        // Transition
        lr.Status = LeaveRequestStatus.Approved;
        lr.ReviewedById = _currentUser.UserId;
        lr.ReviewedAt = DateTime.UtcNow;
        lr.DecisionReason = request.Comment;
        lr.ReviewerComment = request.Comment;
        lr.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveRequests.Update(lr);

        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        // Notify the employee
        await _notifications.SendToUserAsync(
            lr.Employee.UserId,
            "Leave Request Approved ✅",
            $"Your {lr.LeaveType.Name} request ({lr.StartDate:yyyy-MM-dd} to {lr.EndDate:yyyy-MM-dd}) has been approved."
                + (string.IsNullOrWhiteSpace(request.Comment) ? "" : $" Comment: {request.Comment}"),
            ct);

        return Result<bool>.Success(true, "Leave request approved successfully");
    }
}
