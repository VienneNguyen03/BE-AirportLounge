using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record RevokeIdCardCommand(Guid CardId, string Reason) : IRequest<Result<bool>>;

public class RevokeIdCardCommandHandler : IRequestHandler<RevokeIdCardCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public RevokeIdCardCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(RevokeIdCardCommand req, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<bool>.Failure("Only admins or managers can revoke ID cards");

        var card = await _uow.EmployeeIdCards.Query()
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == req.CardId, ct);
            
        if (card is null)
            return Result<bool>.Failure("ID card not found");

        if (card.Status == IdCardStatus.Revoked)
            return Result<bool>.Success(true, "ID card is already revoked");

        var oldStatus = card.Status;
        card.Status = IdCardStatus.Revoked;
        card.RevokedAt = DateTime.UtcNow;
        card.RevokedById = _currentUser.UserId!.Value;
        card.RevokeReason = req.Reason;
        card.UpdatedBy = _currentUser.Email;

        var evt = new IdCardEvent
        {
            CardId = card.Id,
            FromStatus = oldStatus,
            ToStatus = IdCardStatus.Revoked,
            Action = "Revoke",
            Comment = req.Reason,
            PerformedById = _currentUser.UserId!.Value
        };

        _uow.EmployeeIdCards.Update(card);
        await _uow.IdCardEvents.AddAsync(evt, ct);
        
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.IdCards(card.EmployeeId), ct);

        return Result<bool>.Success(true, "ID card revoked successfully");
    }
}
