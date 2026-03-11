using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record RequestIdCardReissueCommand(Guid CardId, string Reason) : IRequest<Result<bool>>;

public class RequestIdCardReissueCommandHandler : IRequestHandler<RequestIdCardReissueCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public RequestIdCardReissueCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(RequestIdCardReissueCommand req, CancellationToken ct)
    {
        var card = await _uow.EmployeeIdCards.Query()
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == req.CardId, ct);
            
        if (card is null)
            return Result<bool>.Failure("ID card not found");

        // Employees can only request reissue for their own cards
        if (_currentUser.Role == "Staff")
        {
            var isOwnCard = await _uow.Employees.Query()
                .AnyAsync(e => e.Id == card.EmployeeId && e.UserId == _currentUser.UserId, ct);
            if (!isOwnCard)
                return Result<bool>.Failure("You can only request reissue for your own ID card");
        }

        if (card.Status == IdCardStatus.ReissueRequested)
            return Result<bool>.Success(true, "A reissue request is already pending for this card");

        if (card.Status != IdCardStatus.Lost && card.Status != IdCardStatus.Damaged && card.Status != IdCardStatus.Active)
            return Result<bool>.Failure($"Cannot request reissue for card in '{card.Status}' status. Must be Lost, Damaged, or Active (close to expiry).");

        var oldStatus = card.Status;
        card.Status = IdCardStatus.ReissueRequested;
        card.UpdatedBy = _currentUser.Email;

        var evt = new IdCardEvent
        {
            CardId = card.Id,
            FromStatus = oldStatus,
            ToStatus = IdCardStatus.ReissueRequested,
            Action = "Request Reissue",
            Comment = req.Reason,
            PerformedById = _currentUser.UserId!.Value
        };

        _uow.EmployeeIdCards.Update(card);
        await _uow.IdCardEvents.AddAsync(evt, ct);
        
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.IdCards(card.EmployeeId), ct);

        return Result<bool>.Success(true, "ID card reissue requested successfully");
    }
}
