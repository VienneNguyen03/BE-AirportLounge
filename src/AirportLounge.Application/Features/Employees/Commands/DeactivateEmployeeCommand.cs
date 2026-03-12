using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Employees.Commands;

public record DeactivateEmployeeCommand(Guid EmployeeId) : IRequest<Result<bool>>;
