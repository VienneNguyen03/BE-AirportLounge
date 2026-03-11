using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record ReissueIdCardCommand(Guid CardId, Guid? NewTemplateId) : IRequest<Result<Guid>>;

public class ReissueIdCardCommandHandler : IRequestHandler<ReissueIdCardCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;
    private readonly IMediator _mediator;

    public ReissueIdCardCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache, IMediator mediator)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
        _mediator = mediator;
    }

    public async Task<Result<Guid>> Handle(ReissueIdCardCommand req, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<Guid>.Failure("Only admins or managers can approve an ID card reissue");

        var oldCard = await _uow.EmployeeIdCards.Query()
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.Id == req.CardId, ct);
            
        if (oldCard is null)
            return Result<Guid>.Failure("Original ID card not found");

        if (oldCard.Status != IdCardStatus.ReissueRequested)
            return Result<Guid>.Failure($"Cannot reissue card that is not in '{IdCardStatus.ReissueRequested}' status.");

        // We need a template for the new card: either the user provides one, or we find the active one with the old name
        Guid templateIdToUse;
        if (req.NewTemplateId.HasValue)
        {
            var t = await _uow.IdCardTemplates.GetByIdAsync(req.NewTemplateId.Value, ct);
            if (t is null || !t.IsActive) return Result<Guid>.Failure("Provided new template not found or inactive");
            templateIdToUse = t.Id;
        }
        else
        {
            var t = await _uow.IdCardTemplates.Query().FirstOrDefaultAsync(x => x.Name == oldCard.TemplateType && x.IsActive, ct);
            if (t is null)
                return Result<Guid>.Failure($"Original template '{oldCard.TemplateType}' is inactive or deleted, and no new template was provided.");
            templateIdToUse = t.Id;
        }

        // 1. Mark old card as Reissued
        oldCard.Status = IdCardStatus.Reissued;
        oldCard.UpdatedBy = _currentUser.Email;

        var evtOld = new IdCardEvent
        {
            CardId = oldCard.Id,
            FromStatus = IdCardStatus.ReissueRequested,
            ToStatus = IdCardStatus.Reissued,
            Action = "Approve Reissue",
            Comment = "Reissue request approved, generating new card",
            PerformedById = _currentUser.UserId!.Value
        };

        _uow.EmployeeIdCards.Update(oldCard);
        await _uow.IdCardEvents.AddAsync(evtOld, ct);
        
        // Save changes for old card first to get clear state
        await _uow.SaveChangesAsync(ct);

        // 2. Issue a NEW card (DraftCard) using the IssueIdCardCommand
        var newIssueResult = await _mediator.Send(new IssueIdCardCommand(oldCard.EmployeeId, templateIdToUse), ct);
        if (!newIssueResult.IsSuccess)
            return Result<Guid>.Failure($"Failed to generate new card draft: {newIssueResult.Message}");

        // 3. Link the old card to the new card
        var newCardId = newIssueResult.Data;
        var newCard = await _uow.EmployeeIdCards.GetByIdAsync(newCardId, ct);
        if (newCard != null)
        {
            oldCard.ReplacedByCardId = newCardId;
            _uow.EmployeeIdCards.Update(oldCard);
            await _uow.SaveChangesAsync(ct);
        }

        await _cache.RemoveAsync(CacheKeys.IdCards(oldCard.EmployeeId), ct);

        return Result<Guid>.Success(newCardId, "ID card reissued successfully. New Draft created.");
    }
}
