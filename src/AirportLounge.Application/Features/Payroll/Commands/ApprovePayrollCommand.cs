using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Payroll.Commands;

public record ApprovePayrollCommand(Guid PayrollRecordId) : IRequest<Result<bool>>;
