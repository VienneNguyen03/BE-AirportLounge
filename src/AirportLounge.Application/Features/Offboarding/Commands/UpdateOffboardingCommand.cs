using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Offboarding.Commands;

public record UpdateOffboardingCommand(
    Guid ProcessId,
    bool? ExitSurveyCompleted,
    bool? AssetReturned,
    bool? AccessRevoked) : IRequest<Result<bool>>;

public class UpdateOffboardingCommandHandler : IRequestHandler<UpdateOffboardingCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateOffboardingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(UpdateOffboardingCommand req, CancellationToken ct)
    {
        var process = await _uow.OffboardingProcesses.Query()
            .Include(o => o.Employee).ThenInclude(e => e.User)
            .FirstOrDefaultAsync(o => o.Id == req.ProcessId, ct);

        if (process is null)
            return Result<bool>.Failure("Offboarding process not found");

        if (process.Status == OffboardingStatus.Completed)
            return Result<bool>.Failure("Offboarding process is already completed");

        if (req.ExitSurveyCompleted.HasValue) process.ExitSurveyCompleted = req.ExitSurveyCompleted.Value;
        if (req.AssetReturned.HasValue) process.AssetReturned = req.AssetReturned.Value;
        if (req.AccessRevoked.HasValue) process.AccessRevoked = req.AccessRevoked.Value;

        if (process.Status == OffboardingStatus.Initiated)
            process.Status = OffboardingStatus.InProgress;

        if (process.ExitSurveyCompleted && process.AssetReturned && process.AccessRevoked)
        {
            process.Status = OffboardingStatus.Completed;

            process.Employee.IsDeleted = true;
            process.Employee.UpdatedBy = _currentUser.Email;
            process.Employee.User.IsActive = false;
            process.Employee.User.UpdatedBy = _currentUser.Email;
            _uow.Employees.Update(process.Employee);

            await _cache.RemoveAsync(CacheKeys.EmployeeById(process.EmployeeId), ct);
        }

        process.UpdatedBy = _currentUser.Email;
        _uow.OffboardingProcesses.Update(process);
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Offboarding(process.EmployeeId), ct);

        var completed = process.Status == OffboardingStatus.Completed;
        return Result<bool>.Success(true, completed ? "Offboarding completed. Employee deactivated" : "Offboarding updated");
    }
}
