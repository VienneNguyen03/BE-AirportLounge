using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Documents.Commands;

public record UploadDocumentCommand(
    Guid EmployeeId,
    string Title,
    DocumentCategory Category,
    string FilePath,
    long FileSize,
    string? ContentType,
    bool IsConfidential) : IRequest<Result<Guid>>;

public class UploadDocumentCommandHandler : IRequestHandler<UploadDocumentCommand, Result<Guid>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UploadDocumentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(UploadDocumentCommand req, CancellationToken ct)
    {
        var employeeExists = await _uow.Employees.ExistsAsync(req.EmployeeId, ct);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var document = new EmployeeDocument
        {
            EmployeeId = req.EmployeeId,
            Title = req.Title,
            Category = req.Category,
            FilePath = req.FilePath,
            FileSize = req.FileSize,
            ContentType = req.ContentType,
            UploadedById = _currentUser.UserId!.Value,
            IsConfidential = req.IsConfidential,
            CreatedBy = _currentUser.Email
        };

        await _uow.EmployeeDocuments.AddAsync(document, ct);
        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Documents(req.EmployeeId), ct);

        return Result<Guid>.Success(document.Id, "Document uploaded");
    }
}
