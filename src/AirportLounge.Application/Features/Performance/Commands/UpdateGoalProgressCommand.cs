using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Performance.Commands;

public record UpdateGoalProgressCommand(
    Guid GoalId,
    decimal CurrentValue,
    GoalStatus? Status) : IRequest<Result<bool>>;

public class UpdateGoalProgressCommandHandler : IRequestHandler<UpdateGoalProgressCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateGoalProgressCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(UpdateGoalProgressCommand request, CancellationToken ct)
    {
        var goal = await _uow.PerformanceGoals.GetByIdAsync(request.GoalId, ct);
        if (goal is null)
            return Result<bool>.Failure("Performance goal not found");

        goal.CurrentValue = request.CurrentValue;
        if (request.Status.HasValue)
            goal.Status = request.Status.Value;
        goal.UpdatedBy = _currentUser.Email;

        _uow.PerformanceGoals.Update(goal);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PerformanceGoals(goal.EmployeeId), ct);

        return Result<bool>.Success(true, "Goal progress updated");
    }
}
