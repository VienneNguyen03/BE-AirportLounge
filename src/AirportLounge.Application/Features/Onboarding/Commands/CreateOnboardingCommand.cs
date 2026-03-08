using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Onboarding.Commands;

public record OnboardingTaskInput(string Title, string? Description, Guid? AssignedToId, DateTime? DueDate, int SortOrder);

public record CreateOnboardingCommand(
    Guid EmployeeId,
    Guid? AssignedMentorId,
    List<OnboardingTaskInput> Tasks) : IRequest<Result<Guid>>;

public class CreateOnboardingCommandHandler : IRequestHandler<CreateOnboardingCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateOnboardingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateOnboardingCommand req, CancellationToken ct)
    {
        var employeeExists = await _uow.Employees.ExistsAsync(req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var alreadyExists = await _uow.OnboardingProcesses.Query()
            .AnyAsync(o => o.EmployeeId == req.EmployeeId && o.Status == Domain.Enums.OnboardingStatus.InProgress, ct);
        if (alreadyExists)
            return Result<Guid>.Failure("Employee already has an active onboarding process");

        var process = new OnboardingProcess
        {
            EmployeeId = req.EmployeeId,
            StartDate = DateTime.UtcNow,
            AssignedMentorId = req.AssignedMentorId,
            CreatedBy = _currentUser.Email
        };

        await _uow.OnboardingProcesses.AddAsync(process, ct);

        foreach (var t in req.Tasks)
        {
            await _uow.OnboardingTasks.AddAsync(new OnboardingTask
            {
                ProcessId = process.Id,
                Title = t.Title,
                Description = t.Description,
                AssignedToId = t.AssignedToId,
                DueDate = t.DueDate,
                SortOrder = t.SortOrder,
                CreatedBy = _currentUser.Email
            }, ct);
        }

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Onboarding(req.EmployeeId), ct);

        return Result<Guid>.Success(process.Id, "Onboarding process created");
    }
}
