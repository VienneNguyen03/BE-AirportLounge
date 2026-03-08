using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Payroll.Commands;

public record CreatePayrollCommand(
    Guid EmployeeId,
    int Year,
    int Month,
    decimal BaseSalary,
    decimal TotalAllowances,
    decimal OvertimePay,
    decimal Bonuses,
    decimal TotalDeductions,
    string? Notes) : IRequest<Result<Guid>>;

public class CreatePayrollCommandHandler : IRequestHandler<CreatePayrollCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreatePayrollCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreatePayrollCommand req, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<Guid>.Failure("Only Admin or Manager can create payroll records");

        if (req.Month is < 1 or > 12)
            return Result<Guid>.Failure("Month must be between 1 and 12");

        var employeeExists = await _uow.Employees.ExistsAsync(req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var duplicate = await _uow.PayrollRecords.Query()
            .AnyAsync(p => p.EmployeeId == req.EmployeeId && p.Year == req.Year && p.Month == req.Month, ct);
        if (duplicate)
            return Result<Guid>.Failure($"Payroll for {req.Month}/{req.Year} already exists for this employee");

        var netSalary = req.BaseSalary + req.TotalAllowances + req.OvertimePay + req.Bonuses - req.TotalDeductions;

        var record = new PayrollRecord
        {
            EmployeeId = req.EmployeeId,
            Year = req.Year,
            Month = req.Month,
            BaseSalary = req.BaseSalary,
            TotalAllowances = req.TotalAllowances,
            OvertimePay = req.OvertimePay,
            Bonuses = req.Bonuses,
            TotalDeductions = req.TotalDeductions,
            NetSalary = Math.Round(netSalary, 2),
            Status = PayrollStatus.Draft,
            Notes = req.Notes,
            CreatedBy = _currentUser.Email
        };

        await _uow.PayrollRecords.AddAsync(record, ct);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PayrollByEmployee(req.EmployeeId, req.Year), ct);

        return Result<Guid>.Success(record.Id, "Payroll record created");
    }
}
