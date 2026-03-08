using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Payroll.Commands;

public record SetSalaryStructureCommand(
    Guid EmployeeId,
    decimal BaseSalary,
    decimal MealAllowance,
    decimal TransportAllowance,
    decimal NightShiftAllowance,
    decimal InsuranceDeduction,
    decimal TaxDeduction,
    DateTime EffectiveFrom) : IRequest<Result<Guid>>;

public class SetSalaryStructureCommandHandler : IRequestHandler<SetSalaryStructureCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public SetSalaryStructureCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(SetSalaryStructureCommand req, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<Guid>.Failure("Only Admin or Manager can set salary structures");

        var employeeExists = await _uow.Employees.ExistsAsync(req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var existing = await _uow.SalaryStructures.Query()
            .FirstOrDefaultAsync(s => s.EmployeeId == req.EmployeeId, ct);

        if (existing is not null)
        {
            existing.BaseSalary = req.BaseSalary;
            existing.MealAllowance = req.MealAllowance;
            existing.TransportAllowance = req.TransportAllowance;
            existing.NightShiftAllowance = req.NightShiftAllowance;
            existing.InsuranceDeduction = req.InsuranceDeduction;
            existing.TaxDeduction = req.TaxDeduction;
            existing.EffectiveFrom = req.EffectiveFrom;
            existing.UpdatedBy = _currentUser.Email;
            _uow.SalaryStructures.Update(existing);
        }
        else
        {
            existing = new SalaryStructure
            {
                EmployeeId = req.EmployeeId,
                BaseSalary = req.BaseSalary,
                MealAllowance = req.MealAllowance,
                TransportAllowance = req.TransportAllowance,
                NightShiftAllowance = req.NightShiftAllowance,
                InsuranceDeduction = req.InsuranceDeduction,
                TaxDeduction = req.TaxDeduction,
                EffectiveFrom = req.EffectiveFrom,
                CreatedBy = _currentUser.Email
            };
            await _uow.SalaryStructures.AddAsync(existing, ct);
        }

        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.SalaryStructure(req.EmployeeId), ct);

        return Result<Guid>.Success(existing.Id, "Salary structure saved");
    }
}
