using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record DeleteLeaveTypeCommand(Guid LeaveTypeId) : IRequest<Result<bool>>;

public class DeleteLeaveTypeCommandHandler : IRequestHandler<DeleteLeaveTypeCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    public DeleteLeaveTypeCommandHandler(IUnitOfWork uow) => _uow = uow;

    public async Task<Result<bool>> Handle(DeleteLeaveTypeCommand req, CancellationToken ct)
    {
        var leaveType = await _uow.LeaveTypes.GetByIdAsync(req.LeaveTypeId, ct);
        if (leaveType is null) return Result<bool>.Failure("Leave type not found");

        var hasRequests = await _uow.LeaveRequests.Query()
            .AnyAsync(lr => lr.LeaveTypeId == req.LeaveTypeId, ct);
        if (hasRequests)
            return Result<bool>.Failure("Cannot delete leave type that has existing requests");

        leaveType.IsDeleted = true;
        _uow.LeaveTypes.Update(leaveType);
        await _uow.SaveChangesAsync(ct);
        return Result<bool>.Success(true, "Leave type deleted");
    }
}
