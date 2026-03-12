using AirportLounge.Application.Common.Models;
using MediatR;

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
