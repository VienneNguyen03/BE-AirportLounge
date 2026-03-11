using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Onboarding.Commands;

public record CompleteOnboardingTaskCommand(Guid TaskId) : IRequest<Result<bool>>;

public class CompleteOnboardingTaskCommandHandler : IRequestHandler<CompleteOnboardingTaskCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CompleteOnboardingTaskCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(CompleteOnboardingTaskCommand req, CancellationToken ct)
    {
        var task = await _uow.OnboardingTasks.Query()
            .Include(t => t.Process)
            .FirstOrDefaultAsync(t => t.Id == req.TaskId, ct);

        if (task is null)
            return Result<bool>.Failure("Onboarding task not found");

        if (task.IsCompleted)
            return Result<bool>.Failure("Task is already completed");

        // Guard: process must be InProgress
        if (task.Process.Status != OnboardingStatus.InProgress)
            return Result<bool>.Failure($"Cannot complete tasks when process is '{task.Process.Status}'");

        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;
        task.UpdatedBy = _currentUser.Email;
        _uow.OnboardingTasks.Update(task);

        // Auto-complete: if all tasks done → mark process Completed
        var allCompleted = await _uow.OnboardingTasks.Query()
            .Where(t => t.ProcessId == task.ProcessId && t.Id != req.TaskId)
            .AllAsync(t => t.IsCompleted, ct);

        if (allCompleted)
        {
            task.Process.Status = OnboardingStatus.Completed;
            task.Process.CompletedDate = DateTime.UtcNow;
            task.Process.UpdatedBy = _currentUser.Email;
            _uow.OnboardingProcesses.Update(task.Process);
        }

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Onboarding(task.Process.EmployeeId), ct);

        return Result<bool>.Success(true, allCompleted ? "Task completed. Onboarding finished" : "Task completed");
    }
}
