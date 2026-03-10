using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Employees.Commands;

public record ResetPasswordCommand(Guid EmployeeId, string NewPassword) : IRequest<Result<bool>>;
