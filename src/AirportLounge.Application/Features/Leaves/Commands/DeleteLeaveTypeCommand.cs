using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record DeleteLeaveTypeCommand(Guid LeaveTypeId) : IRequest<Result<bool>>;

public class DeleteLeaveTypeCommandHandler : IRequestHandler<DeleteLeaveTypeCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICacheService _cache;
    
    public DeleteLeaveTypeCommandHandler(IUnitOfWork uow, ICacheService cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(DeleteLeaveTypeCommand req, CancellationToken ct)
    {
        var leaveType = await _uow.LeaveTypes.GetByIdAsync(req.LeaveTypeId, ct);
        if (leaveType is null) return Result<bool>.Failure("Leave type not found");

        var hasRequests = await _uow.LeaveRequests.Query()
            .AnyAsync(lr => lr.LeaveTypeId == req.LeaveTypeId && !lr.IsDeleted, ct);
        if (hasRequests)
            return Result<bool>.Failure("Cannot delete leave type that has existing requests");

        leaveType.IsDeleted = true;
        _uow.LeaveTypes.Update(leaveType);
        await _uow.SaveChangesAsync(ct);
        
        await _cache.RemoveAsync(CacheKeys.LeaveTypesList, ct);
        
        return Result<bool>.Success(true, "Leave type deleted");
    }
}
