using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record ActivateIdCardCommand(Guid CardId) : IRequest<Result<bool>>;

public class ActivateIdCardCommandHandler : IRequestHandler<ActivateIdCardCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public ActivateIdCardCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(ActivateIdCardCommand req, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<bool>.Failure("Only admins or managers can activate ID cards");

        var card = await _uow.EmployeeIdCards.Query()
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == req.CardId, ct);
            
        if (card is null)
            return Result<bool>.Failure("ID card not found");

        if (card.Status == IdCardStatus.Active)
            return Result<bool>.Success(true, "ID card is already active");

        if (card.Status != IdCardStatus.DraftCard && card.Status != IdCardStatus.Issued)
            return Result<bool>.Failure($"Cannot activate card from status {card.Status}");

        // Automatically revoke any previously active cards for this employee
        var activeCards = await _uow.EmployeeIdCards.Query()
            .Where(c => c.EmployeeId == card.EmployeeId && c.Status == IdCardStatus.Active)
            .ToListAsync(ct);

        foreach (var activeCard in activeCards)
        {
            activeCard.Status = IdCardStatus.Revoked;
            activeCard.RevokedAt = DateTime.UtcNow;
            activeCard.RevokedById = _currentUser.UserId!.Value;
            activeCard.RevokeReason = "Superseded by newly activated card";
            _uow.EmployeeIdCards.Update(activeCard);
        }

        var oldStatus = card.Status;
        card.Status = IdCardStatus.Active;
        card.ActivatedAt = DateTime.UtcNow;
        card.ExpiryDate = DateTime.UtcNow.AddYears(1); // Standard 1 year expiry
        card.UpdatedBy = _currentUser.Email;

        var evt = new IdCardEvent
        {
            CardId = card.Id,
            FromStatus = oldStatus,
            ToStatus = IdCardStatus.Active,
            Action = "Activate Card",
            Comment = "Card activated and ready for use",
            PerformedById = _currentUser.UserId!.Value
        };

        _uow.EmployeeIdCards.Update(card);
        await _uow.IdCardEvents.AddAsync(evt, ct);
        
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.IdCards(card.EmployeeId), ct);

        return Result<bool>.Success(true, "ID card activated successfully");
    }
}
