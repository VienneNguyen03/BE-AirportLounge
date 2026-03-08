using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Documents.Commands;

public record DeleteDocumentCommand(Guid DocumentId) : IRequest<Result<bool>>;

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public DeleteDocumentCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(DeleteDocumentCommand req, CancellationToken ct)
    {
        if (_currentUser.Role is not ("Admin"))
            return Result<bool>.Failure("Only admins can delete documents");

        var document = await _uow.EmployeeDocuments.GetByIdAsync(req.DocumentId, ct);
        if (document is null)
            return Result<bool>.Failure("Document not found");

        document.IsDeleted = true;
        document.UpdatedBy = _currentUser.Email;
        _uow.EmployeeDocuments.Update(document);

        await _uow.SaveChangesAsync(ct);
        await _cache.RemoveAsync(CacheKeys.Documents(document.EmployeeId), ct);

        return Result<bool>.Success(true, "Document deleted");
    }
}
