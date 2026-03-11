using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Reject: UnderReview → Rejected (Manager) ─────────────────────
public record RejectLeaveRequestCommand(
    Guid LeaveRequestId,
    string? Reason) : IRequest<Result<bool>>;

public class RejectLeaveRequestCommandHandler : IRequestHandler<RejectLeaveRequestCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly INotificationService _notifications;

    public RejectLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser,
        ICacheService cache, INotificationService notifications)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
        _notifications = notifications;
    }

    public async Task<Result<bool>> Handle(RejectLeaveRequestCommand request, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<bool>.Failure("Only managers can reject leave requests");

        var lr = await _unitOfWork.LeaveRequests.Query()
            .Include(x => x.Employee).ThenInclude(e => e.User)
            .Include(x => x.LeaveType)
            .FirstOrDefaultAsync(x => x.Id == request.LeaveRequestId && !x.IsDeleted, ct);

        if (lr is null)
            return Result<bool>.Failure("Leave request not found");

        // Idempotent
        if (lr.Status == LeaveRequestStatus.Rejected)
            return Result<bool>.Success(true, "Already rejected");

        if (lr.Status != LeaveRequestStatus.UnderReview)
            return Result<bool>.Failure($"Cannot reject: status is '{lr.Status}'");

        // Transition
        lr.Status = LeaveRequestStatus.Rejected;
        lr.ReviewedById = _currentUser.UserId;
        lr.ReviewedAt = DateTime.UtcNow;
        lr.DecisionReason = request.Reason;
        lr.ReviewerComment = request.Reason;
        lr.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveRequests.Update(lr);

        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        // Notify the employee
        await _notifications.SendToUserAsync(
            lr.Employee.UserId,
            "Leave Request Rejected ❌",
            $"Your {lr.LeaveType.Name} request ({lr.StartDate:yyyy-MM-dd} to {lr.EndDate:yyyy-MM-dd}) has been rejected."
                + (string.IsNullOrWhiteSpace(request.Reason) ? "" : $" Reason: {request.Reason}"),
            ct);

        return Result<bool>.Success(true, "Leave request rejected");
    }
}
