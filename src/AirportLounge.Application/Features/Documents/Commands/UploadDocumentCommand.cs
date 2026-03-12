using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

namespace AirportLounge.Application.Features.Documents.Commands;

public record UploadDocumentCommand(
    Guid EmployeeId,
    string Title,
    DocumentCategory Category,
    string FilePath,
    long FileSize,
    string? ContentType,
    bool IsConfidential) : IRequest<Result<Guid>>;
