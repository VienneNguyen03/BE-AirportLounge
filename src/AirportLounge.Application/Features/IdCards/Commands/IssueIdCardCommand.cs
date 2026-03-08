using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.IdCards.Commands;

public record IssueIdCardCommand(Guid EmployeeId, string? TemplateType) : IRequest<Result<Guid>>;

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
        if (_currentUser.Role is not ("Admin"))
            return Result<Guid>.Failure("Only admins can issue ID cards");

        var employeeExists = await _uow.Employees.ExistsAsync(req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var cardNumber = $"IDC-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        var qrData = $"{req.EmployeeId}|{cardNumber}|{DateTime.UtcNow:O}";

        var card = new EmployeeIdCard
        {
            EmployeeId = req.EmployeeId,
            CardNumber = cardNumber,
            TemplateType = req.TemplateType,
            IssuedAt = DateTime.UtcNow,
            IssuedById = _currentUser.UserId!.Value,
            QrCodeData = qrData,
            CreatedBy = _currentUser.Email
        };

        await _uow.EmployeeIdCards.AddAsync(card, ct);
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.IdCards(req.EmployeeId), ct);

        return Result<Guid>.Success(card.Id, "ID card issued");
    }
}
