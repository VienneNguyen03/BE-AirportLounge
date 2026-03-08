using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record CreateLeaveTypeCommand(
    string Name,
    string? Description,
    int DefaultDaysPerYear,
    bool RequiresDocumentation) : IRequest<Result<Guid>>;

public class CreateLeaveTypeCommandHandler : IRequestHandler<CreateLeaveTypeCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateLeaveTypeCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateLeaveTypeCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role != "Admin")
            return Result<Guid>.Failure("Only administrators can create leave types");

        var exists = await _unitOfWork.LeaveTypes.Query()
            .AnyAsync(lt => lt.Name == request.Name && !lt.IsDeleted, cancellationToken);

        if (exists)
            return Result<Guid>.Failure($"A leave type with name '{request.Name}' already exists");

        var leaveType = new LeaveType
        {
            Name = request.Name,
            Description = request.Description,
            DefaultDaysPerYear = request.DefaultDaysPerYear,
            RequiresDocumentation = request.RequiresDocumentation,
            IsActive = true,
            CreatedBy = _currentUser.Email
        };

        await _unitOfWork.LeaveTypes.AddAsync(leaveType, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync(CacheKeys.LeaveTypesList, cancellationToken);

        return Result<Guid>.Success(leaveType.Id, "Leave type created successfully");
    }
}
