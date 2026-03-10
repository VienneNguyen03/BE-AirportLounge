using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record UpdateLeaveTypeCommand(
    Guid Id,
    string Name,
    string? Description,
    int DefaultDaysPerYear,
    bool RequiresDocumentation
) : IRequest<Result<bool>>;

public class UpdateLeaveTypeCommandHandler : IRequestHandler<UpdateLeaveTypeCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateLeaveTypeCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(UpdateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != "Admin")
            return Result<bool>.Failure("Only administrators can update leave types");

        var leaveType = await _uow.LeaveTypes.GetByIdAsync(request.Id, cancellationToken);
        
        if (leaveType == null || leaveType.IsDeleted)
        {
            return Result<bool>.Failure($"Leave type with ID {request.Id} not found.");
        }

        // Check for duplicate name (excluding itself)
        var exists = await _uow.LeaveTypes.Query()
            .AnyAsync(lt => lt.Name.ToLower() == request.Name.ToLower() && lt.Id != request.Id && !lt.IsDeleted, cancellationToken);
            
        if (exists)
        {
            return Result<bool>.Failure($"A leave type with the name '{request.Name}' already exists.");
        }

        leaveType.Name = request.Name;
        leaveType.Description = request.Description;
        leaveType.DefaultDaysPerYear = request.DefaultDaysPerYear;
        leaveType.RequiresDocumentation = request.RequiresDocumentation;
        leaveType.UpdatedBy = _currentUser.Email;
        
        _uow.LeaveTypes.Update(leaveType);
        await _uow.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.LeaveTypesList, cancellationToken);

        return Result<bool>.Success(true);
    }
}
