using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record ReportIdCardIssueCommand(Guid CardId, bool IsLost, string Reason) : IRequest<Result<bool>>;

public class ReportIdCardIssueCommandHandler : IRequestHandler<ReportIdCardIssueCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public ReportIdCardIssueCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(ReportIdCardIssueCommand req, CancellationToken ct)
    {
        var card = await _uow.EmployeeIdCards.Query()
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == req.CardId, ct);
            
        if (card is null)
            return Result<bool>.Failure("ID card not found");

        // Employees can only report their own cards, managers/admins can report any
        if (_currentUser.Role == "Staff")
        {
            var isOwnCard = await _uow.Employees.Query()
                .AnyAsync(e => e.Id == card.EmployeeId && e.UserId == _currentUser.UserId, ct);
            if (!isOwnCard)
                return Result<bool>.Failure("You can only report issues for your own ID card");
        }

        var targetStatus = req.IsLost ? IdCardStatus.Lost : IdCardStatus.Damaged;

        if (card.Status == targetStatus)
            return Result<bool>.Success(true, $"ID card is already reported as {targetStatus}");

        if (card.Status != IdCardStatus.Active)
            return Result<bool>.Failure($"Can only report Lost/Damaged for Active cards. Current status: {card.Status}");

        var oldStatus = card.Status;
        card.Status = targetStatus;
        card.UpdatedBy = _currentUser.Email;

        var evtName = req.IsLost ? "Report Lost" : "Report Damaged";
        
        var evt = new IdCardEvent
        {
            CardId = card.Id,
            FromStatus = oldStatus,
            ToStatus = targetStatus,
            Action = evtName,
            Comment = req.Reason,
            PerformedById = _currentUser.UserId!.Value
        };

        _uow.EmployeeIdCards.Update(card);
        await _uow.IdCardEvents.AddAsync(evt, ct);
        
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.IdCards(card.EmployeeId), ct);

        return Result<bool>.Success(true, $"ID card successfully reported as {targetStatus}");
    }
}
