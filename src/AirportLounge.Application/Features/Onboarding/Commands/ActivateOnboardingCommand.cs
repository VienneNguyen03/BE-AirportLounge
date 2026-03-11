using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Onboarding.Commands;

public record ActivateOnboardingCommand(Guid ProcessId) : IRequest<Result<bool>>;

public class ActivateOnboardingCommandHandler : IRequestHandler<ActivateOnboardingCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public ActivateOnboardingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(ActivateOnboardingCommand req, CancellationToken ct)
    {
        var process = await _uow.OnboardingProcesses.GetByIdAsync(req.ProcessId, ct);
        if (process is null)
            return Result<bool>.Failure("Onboarding process not found");

        if (process.Status != OnboardingStatus.Completed)
            return Result<bool>.Failure($"Cannot activate onboarding from status '{process.Status}'");

        process.Status = OnboardingStatus.Activated;
        process.UpdatedBy = _currentUser.Email;
        _uow.OnboardingProcesses.Update(process);

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Onboarding(process.EmployeeId), ct);

        return Result<bool>.Success(true, "Onboarding activated — employee fully onboarded");
    }
}
