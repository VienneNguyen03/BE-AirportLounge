using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Performance.Commands;

public record CreatePerformanceGoalCommand(
    Guid EmployeeId,
    string Title,
    string? Description,
    decimal TargetValue,
    string? Unit,
    DateTime DueDate) : IRequest<Result<Guid>>;

public class CreatePerformanceGoalCommandHandler : IRequestHandler<CreatePerformanceGoalCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreatePerformanceGoalCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreatePerformanceGoalCommand request, CancellationToken ct)
    {
        var employee = await _uow.Employees.GetByIdAsync(request.EmployeeId, ct);
        if (employee is null)
            return Result<Guid>.Failure("Employee not found");

        var goal = new PerformanceGoal
        {
            EmployeeId = request.EmployeeId,
            Title = request.Title,
            Description = request.Description,
            TargetValue = request.TargetValue,
            CurrentValue = 0,
            Unit = request.Unit,
            DueDate = request.DueDate,
            Status = GoalStatus.NotStarted,
            CreatedBy = _currentUser.Email
        };

        await _uow.PerformanceGoals.AddAsync(goal, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PerformanceGoals(request.EmployeeId), ct);

        return Result<Guid>.Success(goal.Id, "Performance goal created");
    }
}
