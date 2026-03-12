using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Payroll.Queries;

public record GetSalaryStructureQuery(Guid EmployeeId) : IRequest<Result<SalaryStructureDto>>;

public record SalaryStructureDto(
    Guid Id,
    Guid EmployeeId,
    decimal BaseSalary,
    decimal MealAllowance,
    decimal TransportAllowance,
    decimal NightShiftAllowance,
    decimal InsuranceDeduction,
    decimal TaxDeduction,
    DateTime EffectiveFrom);
