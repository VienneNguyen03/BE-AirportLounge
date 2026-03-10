using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record CancelLeaveRequestCommand(Guid LeaveRequestId) : IRequest<Result<bool>>;

public class CancelLeaveRequestCommandHandler : IRequestHandler<CancelLeaveRequestCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public CancelLeaveRequestCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(CancelLeaveRequestCommand req, CancellationToken ct)
    {
        var request = await _uow.LeaveRequests.GetByIdAsync(req.LeaveRequestId, ct);
        if (request is null) return Result<bool>.Failure("Leave request not found");

        if (request.Status != LeaveRequestStatus.Pending)
            return Result<bool>.Failure("Only pending requests can be cancelled");

        request.Status = LeaveRequestStatus.Cancelled;
        _uow.LeaveRequests.Update(request);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true, "Leave request cancelled");
    }
}

public record DeleteLeaveRequestsBatchCommand(IReadOnlyList<Guid> LeaveRequestIds) : IRequest<Result<int>>;

public class DeleteLeaveRequestsBatchCommandHandler : IRequestHandler<DeleteLeaveRequestsBatchCommand, Result<int>>
{
    private readonly IUnitOfWork _uow;

    public DeleteLeaveRequestsBatchCommandHandler(IUnitOfWork uow) => _uow = uow;

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
        return Result<int>.Success(requests.Count, $"Deleted {requests.Count} leave requests");
    }
}
