using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Offboarding.Commands;

public record CompleteOffboardingTaskCommand(Guid TaskId) : IRequest<Result<bool>>;

public class CompleteOffboardingTaskCommandHandler : IRequestHandler<CompleteOffboardingTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CompleteOffboardingTaskCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(CompleteOffboardingTaskCommand req, CancellationToken ct)
    {
        var task = await _uow.OffboardingTasks.Query()
            .Include(t => t.Process)
            .FirstOrDefaultAsync(t => t.Id == req.TaskId, ct);

        if (task is null)
            return Result<bool>.Failure("Offboarding task not found");

        if (task.IsCompleted)
            return Result<bool>.Failure("Task is already completed");

        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedBy = _currentUser.Email;
        _uow.OffboardingTasks.Update(task);

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Offboarding(task.Process.EmployeeId), ct);

        return Result<bool>.Success(true, "Offboarding task completed");
    }
}
