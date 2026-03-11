using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

// ── Cancel: Draft | Submitted | Approved | Scheduled → Cancelled ─
public record CancelLeaveRequestCommand(Guid LeaveRequestId) : IRequest<Result<bool>>;

public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CancelLeaveRequestCommandHandler(
        IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(CancelLeaveRequestCommand req, CancellationToken ct)
    {
        var lr = await _unitOfWork.LeaveRequests.GetByIdAsync(req.LeaveRequestId, ct);
        if (lr is null || lr.IsDeleted)
            return Result<bool>.Failure("Leave request not found");

        // Idempotent
        if (lr.Status == LeaveRequestStatus.Cancelled)
            return Result<bool>.Success(true, "Already cancelled");

        var cancellableStatuses = new[]
        {
            LeaveRequestStatus.Draft,
            LeaveRequestStatus.Submitted,
            LeaveRequestStatus.Approved,
            LeaveRequestStatus.Scheduled
        };

        if (!cancellableStatuses.Contains(lr.Status))
            return Result<bool>.Failure($"Cannot cancel a request in '{lr.Status}' status");

        // Guard: for Approved/Scheduled, must be before start date
        if (lr.Status is LeaveRequestStatus.Approved or LeaveRequestStatus.Scheduled)
        {
            if (DateTime.UtcNow.Date >= lr.StartDate.Date)
                return Result<bool>.Failure("Cannot cancel: leave period has already started");
        }

        // Side effect: release reserved balance if Approved/Scheduled
        if (lr.Status is LeaveRequestStatus.Approved or LeaveRequestStatus.Scheduled)
        {
            var balance = await _unitOfWork.LeaveBalances.Query()
                .FirstOrDefaultAsync(lb =>
                    lb.EmployeeId == lr.EmployeeId &&
                    lb.LeaveTypeId == lr.LeaveTypeId &&
                    lb.Year == lr.StartDate.Year &&
                    !lb.IsDeleted, ct);

            if (balance is not null)
            {
                balance.ReservedDays = Math.Max(0, balance.ReservedDays - lr.TotalDays);
                balance.UpdatedBy = _currentUser.Email;
                _unitOfWork.LeaveBalances.Update(balance);
            }
        }

        lr.Status = LeaveRequestStatus.Cancelled;
        lr.UpdatedBy = _currentUser.Email;
        _unitOfWork.LeaveRequests.Update(lr);

        await _unitOfWork.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        return Result<bool>.Success(true, "Leave request cancelled");
    }
}

// ── Batch Delete (unchanged) ─────────────────────────────────────
public record DeleteLeaveRequestsBatchCommand(IReadOnlyList<Guid> LeaveRequestIds) : IRequest<Result<int>>;

public class DeleteLeaveRequestsBatchCommandHandler : IRequestHandler<DeleteLeaveRequestsBatchCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;

    public DeleteLeaveRequestsBatchCommandHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<int>> Handle(DeleteLeaveRequestsBatchCommand req, CancellationToken ct)
    {
        if (req.LeaveRequestIds is null || req.LeaveRequestIds.Count == 0)
            return Result<int>.Failure("No leave request IDs provided");

        var ids = req.LeaveRequestIds.Distinct().ToList();
        var requests = await _uow.LeaveRequests.Query()
            .Where(lr => ids.Contains(lr.Id))
            .ToListAsync(ct);

        if (requests.Count == 0)
            return Result<int>.Failure("No matching leave requests found");

        foreach (var r in requests)
        {
            r.IsDeleted = true;
            _uow.LeaveRequests.Update(r);
        }

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, ct);

        return Result<int>.Success(requests.Count, $"Deleted {requests.Count} leave requests");
    }
}
