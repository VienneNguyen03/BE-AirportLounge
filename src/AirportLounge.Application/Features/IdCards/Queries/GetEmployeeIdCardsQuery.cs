using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.IdCards.Queries;

public record IdCardDto(
    Guid Id, string CardNumber, string? TemplateType, IdCardStatus Status,
    DateTime IssuedAt, DateTime? RevokedAt, string? RevokeReason, string? QrCodeData);

public record GetEmployeeIdCardsQuery(Guid EmployeeId) : IRequest<Result<List<IdCardDto>>>;

public class GetEmployeeIdCardsQueryHandler : IRequestHandler<GetEmployeeIdCardsQuery, Result<List<IdCardDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetEmployeeIdCardsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<List<IdCardDto>>> Handle(GetEmployeeIdCardsQuery req, CancellationToken ct)
    {
        if (_currentUser.Role == "Staff")
        {
            var own = await _uow.Employees.Query()
                .AnyAsync(e => e.Id == req.EmployeeId && e.UserId == _currentUser.UserId, ct);
            if (!own)
                return Result<List<IdCardDto>>.Failure("You can only view your own ID cards");
        }

        var cacheKey = CacheKeys.IdCards(req.EmployeeId);
        var cached = await _cache.GetAsync<List<IdCardDto>>(cacheKey, ct);
        if (cached is not null)
            return Result<List<IdCardDto>>.Success(cached);

        var cards = await _uow.EmployeeIdCards.Query()
            .Where(c => c.EmployeeId == req.EmployeeId && !c.IsDeleted)
            .OrderByDescending(c => c.IssuedAt)
            .Select(c => new IdCardDto(
                c.Id, c.CardNumber, c.TemplateType, c.Status,
                c.IssuedAt, c.RevokedAt, c.RevokeReason, c.QrCodeData))
            .ToListAsync(ct);

        await _cache.SetAsync(cacheKey, cards, CacheKeys.IdCardsTtl, ct);
        return Result<List<IdCardDto>>.Success(cards);
    }
}
