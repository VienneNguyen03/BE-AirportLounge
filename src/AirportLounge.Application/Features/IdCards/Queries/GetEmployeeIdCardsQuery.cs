using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Queries;

public record IdCardEventDto(
    Guid Id,
    IdCardStatus FromStatus,
    IdCardStatus ToStatus,
    string Action,
    string? Comment,
    DateTime PerformedAt,
    string PerformedByName);

public record IdCardDto(
    Guid Id, 
    string CardNumber, 
    string? TemplateType, 
    Guid? TemplateId,
    string? LayoutData,
    string? PrimaryColor,
    IdCardStatus Status,
    DateTime? IssuedAt,
    DateTime? ActivatedAt,
    DateTime? RevokedAt, 
    string? StatusReason, 
    string? QrCodeData,
    DateTime? ExpiryDate,
    Guid? ReplacedByCardId,
    List<IdCardEventDto> Events);

public record GetEmployeeIdCardsQuery(Guid EmployeeId) : IRequest<Result<List<IdCardDto>>>;
