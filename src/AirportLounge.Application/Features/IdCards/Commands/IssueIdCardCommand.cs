using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record IssueIdCardCommand(Guid EmployeeId, Guid TemplateId) : IRequest<Result<Guid>>;

public class IssueIdCardCommandHandler : IRequestHandler<IssueIdCardCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public IssueIdCardCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(IssueIdCardCommand req, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<Guid>.Failure("Only admins or managers can issue ID cards");

        var employeeExists = await _uow.Employees.ExistsAsync(req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var template = await _uow.IdCardTemplates.GetByIdAsync(req.TemplateId, ct);
        if (template is null || !template.IsActive)
            return Result<Guid>.Failure("Template not found or inactive");

        // Cancel any existing Draft requests for this employee to prevent duplicates
        var existingDrafts = await _uow.EmployeeIdCards.Query()
            .Where(c => c.EmployeeId == req.EmployeeId && c.Status == IdCardStatus.DraftCard)
            .ToListAsync(ct);
        
        foreach (var draft in existingDrafts)
        {
            draft.Status = IdCardStatus.Revoked;
            draft.RevokeReason = "Superseded by new issue layout request";
            draft.RevokedAt = DateTime.UtcNow;
            draft.RevokedById = _currentUser.UserId!.Value;
            _uow.EmployeeIdCards.Update(draft);
        }

        var cardNumber = $"IDC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        
        // QR Data: Format EmployeeId|CardNumber|TemplateName|Timestamp
        var qrData = $"{req.EmployeeId}|{cardNumber}|{template.Name}|{DateTime.UtcNow:O}";

        var card = new EmployeeIdCard
        {
            EmployeeId = req.EmployeeId,
            CardNumber = cardNumber,
            TemplateType = template.Name,
            Status = IdCardStatus.DraftCard, // Start as Draft
            QrCodeData = qrData,
            CreatedBy = _currentUser.Email
        };

        var evt = new IdCardEvent
        {
            Card = card,
            FromStatus = IdCardStatus.DraftCard,
            ToStatus = IdCardStatus.DraftCard,
            Action = "Issue Requested",
            Comment = $"ID Card requested using template: {template.Name}",
            PerformedById = _currentUser.UserId!.Value
        };

        card.Events.Add(evt);

        await _uow.EmployeeIdCards.AddAsync(card, ct);
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.IdCards(req.EmployeeId), ct);

        return Result<Guid>.Success(card.Id, "ID card issue requested successfully");
    }
}
