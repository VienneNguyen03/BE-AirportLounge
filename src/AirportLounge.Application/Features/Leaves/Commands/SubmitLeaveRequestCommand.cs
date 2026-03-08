using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record SubmitLeaveRequestCommand(
    Guid LeaveTypeId,
    DateTime StartDate,
    DateTime EndDate,
    string? Reason) : IRequest<Result<Guid>>;

public class SubmitLeaveRequestCommandHandler : IRequestHandler<SubmitLeaveRequestCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;

    public SubmitLeaveRequestCommandHandler(
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

    public async Task<Result<Guid>> Handle(SubmitLeaveRequestCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is null)
            return Result<Guid>.Failure("User not authenticated");

        var employee = await _unitOfWork.Employees.Query()
            .FirstOrDefaultAsync(e => e.UserId == _currentUser.UserId.Value && !e.IsDeleted, cancellationToken);

        if (employee is null)
            return Result<Guid>.Failure("Employee profile not found");

        if (request.StartDate.Date > request.EndDate.Date)
            return Result<Guid>.Failure("Start date must be on or before end date");

        if (request.StartDate.Date < DateTime.UtcNow.Date)
            return Result<Guid>.Failure("Cannot submit leave requests for past dates");

        var leaveType = await _unitOfWork.LeaveTypes.GetByIdAsync(request.LeaveTypeId, cancellationToken);
        if (leaveType is null || !leaveType.IsActive)
            return Result<Guid>.Failure("Leave type not found or inactive");

        var totalDays = (decimal)(request.EndDate.Date - request.StartDate.Date).TotalDays + 1;

        var balance = await _unitOfWork.LeaveBalances.Query()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == employee.Id &&
                lb.LeaveTypeId == request.LeaveTypeId &&
                lb.Year == request.StartDate.Year &&
                !lb.IsDeleted, cancellationToken);

        if (balance is null)
            return Result<Guid>.Failure("No leave balance configured for this leave type and year");

        var pendingDays = await _unitOfWork.LeaveRequests.Query()
            .Where(lr =>
                lr.EmployeeId == employee.Id &&
                lr.LeaveTypeId == request.LeaveTypeId &&
                lr.Status == LeaveRequestStatus.Pending &&
                lr.StartDate.Year == request.StartDate.Year &&
                !lr.IsDeleted)
            .SumAsync(lr => lr.TotalDays, cancellationToken);

        if (balance.RemainingDays - pendingDays < totalDays)
            return Result<Guid>.Failure(
                $"Insufficient leave balance. Available: {balance.RemainingDays - pendingDays} days, Requested: {totalDays} days");

        var hasShiftConflict = await _unitOfWork.ShiftAssignments.Query()
            .AnyAsync(sa =>
                sa.EmployeeId == employee.Id &&
                sa.Date >= request.StartDate.Date &&
                sa.Date <= request.EndDate.Date &&
                !sa.IsDeleted, cancellationToken);

        if (hasShiftConflict)
            return Result<Guid>.Failure("You have shift assignments during the requested leave period. Please resolve conflicts first");

        var leaveRequest = new LeaveRequest
        {
            EmployeeId = employee.Id,
            LeaveTypeId = request.LeaveTypeId,
            StartDate = request.StartDate.Date,
            EndDate = request.EndDate.Date,
            TotalDays = totalDays,
            Reason = request.Reason,
            Status = LeaveRequestStatus.Pending,
            CreatedBy = _currentUser.Email
        };

        await _unitOfWork.LeaveRequests.AddAsync(leaveRequest, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.LeaveRequests(employee.Id), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.LeaveRequestsPending, cancellationToken);

        var managers = await _unitOfWork.Users.Query()
            .Where(u => (u.Role == UserRole.Manager || u.Role == UserRole.Admin) && u.IsActive && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

        if (managers.Count > 0)
        {
            var employeeName = await _unitOfWork.Users.Query()
                .Where(u => u.Id == _currentUser.UserId.Value)
                .Select(u => u.FullName)
                .FirstOrDefaultAsync(cancellationToken) ?? "An employee";

            await _notifications.SendToGroupAsync(
                managers,
                "New Leave Request",
                $"{employeeName} has submitted a {leaveType.Name} request for {request.StartDate:yyyy-MM-dd} to {request.EndDate:yyyy-MM-dd} ({totalDays} days)",
                cancellationToken);
        }

        return Result<Guid>.Success(leaveRequest.Id, "Leave request submitted successfully");
    }
}
