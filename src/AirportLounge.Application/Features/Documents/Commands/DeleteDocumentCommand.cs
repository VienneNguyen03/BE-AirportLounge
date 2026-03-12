using AirportLounge.Application.Common.Models;
using MediatR;

namespace AirportLounge.Application.Features.Documents.Commands;

public record DeleteDocumentCommand(Guid DocumentId) : IRequest<Result<bool>>;
