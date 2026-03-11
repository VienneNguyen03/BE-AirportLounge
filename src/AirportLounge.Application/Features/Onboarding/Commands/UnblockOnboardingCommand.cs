using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Onboarding.Commands;

public record UnblockOnboardingCommand(Guid ProcessId) : IRequest<Result<bool>>;

public class UnblockOnboardingCommandHandler : IRequestHandler<UnblockOnboardingCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UnblockOnboardingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(UnblockOnboardingCommand req, CancellationToken ct)
    {
        var process = await _uow.OnboardingProcesses.GetByIdAsync(req.ProcessId, ct);
        if (process is null)
            return Result<bool>.Failure("Onboarding process not found");

        if (process.Status != OnboardingStatus.Blocked)
            return Result<bool>.Failure($"Cannot unblock onboarding from status '{process.Status}'");

        process.Status = OnboardingStatus.InProgress;
        process.Notes = null;
        process.UpdatedBy = _currentUser.Email;
        _uow.OnboardingProcesses.Update(process);

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Onboarding(process.EmployeeId), ct);

        return Result<bool>.Success(true, "Onboarding unblocked");
    }
}
