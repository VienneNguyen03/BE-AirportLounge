using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record ReportIdCardIssueCommand(Guid CardId, bool IsLost, string Reason) : IRequest<Result<bool>>;
