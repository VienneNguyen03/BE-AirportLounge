using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Documents.Queries;

public record DocumentDto(
    Guid Id, string Title, DocumentCategory Category, string FilePath,
    long FileSize, string? ContentType, bool IsConfidential, DateTime CreatedAt);

public record GetEmployeeDocumentsQuery(Guid EmployeeId) : IRequest<Result<List<DocumentDto>>>;

public class GetEmployeeDocumentsQueryHandler : IRequestHandler<GetEmployeeDocumentsQuery, Result<List<DocumentDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetEmployeeDocumentsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<List<DocumentDto>>> Handle(GetEmployeeDocumentsQuery req, CancellationToken ct)
    {
        var isStaff = _currentUser.Role == "Staff";

        if (isStaff)
        {
            var own = await _uow.Employees.Query()
                .AnyAsync(e => e.Id == req.EmployeeId && e.UserId == _currentUser.UserId, ct);
            if (!own)
                return Result<List<DocumentDto>>.Failure("You can only view your own documents");
        }

        var cacheKey = CacheKeys.Documents(req.EmployeeId);
        var cached = await _cache.GetAsync<List<DocumentDto>>(cacheKey, ct);
        if (cached is not null && !isStaff)
            return Result<List<DocumentDto>>.Success(cached);

        var query = _uow.EmployeeDocuments.Query()
            .Where(d => d.EmployeeId == req.EmployeeId && !d.IsDeleted);

        if (isStaff)
            query = query.Where(d => !d.IsConfidential);

        var docs = await query
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DocumentDto(
                d.Id, d.Title, d.Category, d.FilePath,
                d.FileSize, d.ContentType, d.IsConfidential, d.CreatedAt))
            .ToListAsync(ct);

        if (!isStaff)
            await _cache.SetAsync(cacheKey, docs, CacheKeys.DocumentsTtl, ct);

        return Result<List<DocumentDto>>.Success(docs);
    }
}
