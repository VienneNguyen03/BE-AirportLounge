using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Submit: Draft | NeedsInfo → Submitted ────────────────────────
public record SubmitLeaveRequestCommand(Guid LeaveRequestId) : IRequest<Result<bool>>;

public class SubmitLeaveRequestCommandHandler : IRequestHandler<SubmitLeaveRequestCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;

    public SubmitLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICacheService cache, INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(SubmitLeaveRequestCommand request, CancellationToken ct)
    {
        var lr = await _unitOfWork.LeaveRequests.Query()
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == request.LeaveRequestId && !x.IsDeleted, ct);

        if (lr is null)
            return Result<bool>.Failure("Leave request not found");

        // Idempotent: already Submitted
        if (lr.Status == LeaveRequestStatus.Submitted)
            return Result<bool>.Success(true, "Leave request already submitted");

        if (lr.Status is not (LeaveRequestStatus.Draft or LeaveRequestStatus.NeedsInfo))
            return Result<bool>.Failure($"Cannot submit a request in '{lr.Status}' status");

        // Guard: date range
        if (lr.StartDate.Date < DateTime.UtcNow.Date)
            return Result<bool>.Failure("Cannot submit leave requests for past dates");

        // Guard: leave balance
        var balance = await _unitOfWork.LeaveBalances.Query()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == lr.EmployeeId &&
                lb.LeaveTypeId == lr.LeaveTypeId &&
                lb.Year == lr.StartDate.Year &&
                !lb.IsDeleted, ct);

        if (balance is null)
            return Result<bool>.Failure("No leave balance configured for this leave type and year");

        if (balance.RemainingDays < lr.TotalDays)
            return Result<bool>.Failure(
                $"Insufficient leave balance. Available: {balance.RemainingDays} days, Requested: {lr.TotalDays} days");

        // Guard: shift conflict check
        var hasShiftConflict = await _unitOfWork.ShiftAssignments.Query()
            .AnyAsync(sa =>
                sa.EmployeeId == lr.EmployeeId &&
                sa.Date >= lr.StartDate.Date &&
                sa.Date <= lr.EndDate.Date &&
                !sa.IsDeleted, ct);

        // Transition
        lr.Status = LeaveRequestStatus.Submitted;
        lr.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveRequests.Update(lr);
        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        // Side effect: notify managers
        var managers = await _unitOfWork.Users.Query()
            .Where(u => (u.Role == UserRole.Manager || u.Role == UserRole.Admin) && u.IsActive && !u.IsDeleted)
            .Select(u => u.Id)
            .ToListAsync(ct);

        if (managers.Count > 0)
        {
            var conflictNote = hasShiftConflict ? " ⚠️ Has shift conflicts!" : "";
            await _notifications.SendToGroupAsync(
                managers,
                "New Leave Request Submitted",
                $"{lr.Employee.User.FullName} submitted a {lr.LeaveType.Name} request ({lr.StartDate:yyyy-MM-dd} to {lr.EndDate:yyyy-MM-dd}, {lr.TotalDays} days).{conflictNote}",
                ct);
        }

        return Result<bool>.Success(true, "Leave request submitted successfully");
    }
}
