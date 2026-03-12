using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

namespace AirportLounge.Application.Features.Documents.Queries;

public record DocumentDto(
    Guid Id, string Title, DocumentCategory Category, string FilePath,
    long FileSize, string? ContentType, bool IsConfidential, DateTime CreatedAt);

public record GetEmployeeDocumentsQuery(Guid EmployeeId) : IRequest<Result<List<DocumentDto>>>;
