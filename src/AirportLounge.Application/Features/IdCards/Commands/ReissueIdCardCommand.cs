using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record ReissueIdCardCommand(Guid CardId, Guid? NewTemplateId) : IRequest<Result<Guid>>;
