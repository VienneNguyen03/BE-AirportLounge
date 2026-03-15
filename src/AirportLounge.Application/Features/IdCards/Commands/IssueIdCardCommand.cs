using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record IssueIdCardCommand(Guid EmployeeId, Guid TemplateId) : IRequest<Result<Guid>>;
