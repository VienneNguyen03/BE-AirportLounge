using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Offboarding.Commands;

public record TransitionOffboardingCommand(Guid ProcessId, string Action) : IRequest<Result<bool>>;

public class TransitionOffboardingCommandHandler : IRequestHandler<TransitionOffboardingCommand, Result<bool>>
{
    private static readonly Dictionary<(OffboardingStatus From, string Action), OffboardingStatus> _transitions = new()
    {
        { (OffboardingStatus.Initiated, "start"), OffboardingStatus.InProgress },
        { (OffboardingStatus.InProgress, "asset-recovery"), OffboardingStatus.AssetRecovery },
        { (OffboardingStatus.AssetRecovery, "access-revocation"), OffboardingStatus.AccessRevocation },
        { (OffboardingStatus.AccessRevocation, "final-settlement"), OffboardingStatus.FinalSettlement },
        { (OffboardingStatus.FinalSettlement, "complete"), OffboardingStatus.Completed },
    };

    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public TransitionOffboardingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(TransitionOffboardingCommand req, CancellationToken ct)
    {
        var process = await _uow.OffboardingProcesses.GetByIdAsync(req.ProcessId, ct);
        if (process is null)
            return Result<bool>.Failure("Offboarding process not found");

        var key = (process.Status, req.Action);
        if (!_transitions.TryGetValue(key, out var nextStatus))
            return Result<bool>.Failure($"Invalid transition: cannot '{req.Action}' from status '{process.Status}'");

        process.Status = nextStatus;
        process.UpdatedBy = _currentUser.Email;

        if (nextStatus == OffboardingStatus.AssetRecovery)
            process.AssetReturned = false;
        if (nextStatus == OffboardingStatus.AccessRevocation)
            process.AccessRevoked = false;

        _uow.OffboardingProcesses.Update(process);
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Offboarding(process.EmployeeId), ct);

        return Result<bool>.Success(true, $"Offboarding moved to '{nextStatus}'");
    }
}
