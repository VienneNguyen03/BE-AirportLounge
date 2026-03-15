using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record RequestIdCardReissueCommand(Guid CardId, string Reason) : IRequest<Result<bool>>;
