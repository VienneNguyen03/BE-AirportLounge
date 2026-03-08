using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Offboarding.Commands;

public record InitiateOffboardingCommand(
    Guid EmployeeId,
    DateTime ResignationDate,
    DateTime LastWorkingDate,
    string? Reason) : IRequest<Result<Guid>>;

public class InitiateOffboardingCommandHandler : IRequestHandler<InitiateOffboardingCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public InitiateOffboardingCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(InitiateOffboardingCommand req, CancellationToken ct)
    {
        var employeeExists = await _uow.Employees.ExistsAsync(req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var active = await _uow.OffboardingProcesses.Query()
            .AnyAsync(o => o.EmployeeId == req.EmployeeId && o.Status != OffboardingStatus.Completed, ct);
        if (active)
            return Result<Guid>.Failure("Employee already has an active offboarding process");

        var process = new OffboardingProcess
        {
            EmployeeId = req.EmployeeId,
            ResignationDate = req.ResignationDate,
            LastWorkingDate = req.LastWorkingDate,
            Reason = req.Reason,
            CreatedBy = _currentUser.Email
        };

        await _uow.OffboardingProcesses.AddAsync(process, ct);
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Offboarding(req.EmployeeId), ct);

        return Result<Guid>.Success(process.Id, "Offboarding initiated");
    }
}
