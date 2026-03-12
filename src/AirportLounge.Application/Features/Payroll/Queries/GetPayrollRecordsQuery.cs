using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

namespace AirportLounge.Application.Features.Payroll.Queries;

public record GetPayrollRecordsQuery(Guid EmployeeId, int Year) : IRequest<Result<List<PayrollRecordDto>>>;

public record PayrollRecordDto(
    Guid Id,
    Guid EmployeeId,
    int Year,
    int Month,
    decimal BaseSalary,
    decimal TotalAllowances,
    decimal OvertimePay,
    decimal Bonuses,
    decimal TotalDeductions,
    decimal NetSalary,
    decimal WorkedHours,
    decimal OvertimeHours,
    int UnpaidLeaveDays,
    PayrollStatus Status,
    DateTime? PaidAt,
    string? Notes);
