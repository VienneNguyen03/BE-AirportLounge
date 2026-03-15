using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record ActivateIdCardCommand(Guid CardId) : IRequest<Result<bool>>;
