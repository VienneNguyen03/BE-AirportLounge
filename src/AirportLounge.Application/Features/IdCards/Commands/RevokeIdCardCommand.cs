using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record RevokeIdCardCommand(Guid CardId, string Reason) : IRequest<Result<bool>>;
