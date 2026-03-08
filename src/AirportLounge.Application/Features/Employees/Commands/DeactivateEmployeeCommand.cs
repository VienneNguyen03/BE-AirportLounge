using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Employees.Commands;

public record DeactivateEmployeeCommand(Guid EmployeeId) : IRequest<Result<bool>>;

public class DeactivateEmployeeCommandHandler : IRequestHandler<DeactivateEmployeeCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public DeactivateEmployeeCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(DeactivateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return Result<bool>.Failure("Employee not found");

        employee.User.IsActive = false;
        employee.User.UpdatedBy = _currentUser.Email;
        employee.IsDeleted = true;
        employee.UpdatedBy = _currentUser.Email;

        _unitOfWork.Employees.Update(employee);

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            EntityName = nameof(Employee),
            EntityId = employee.Id,
            Action = "Deactivated",
            PerformedBy = _currentUser.Email ?? "System",
            PerformedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate caches affected by this employee's deactivation
        await _cache.RemoveAsync(CacheKeys.EmployeeById(request.EmployeeId), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.StaffDashboard(request.EmployeeId), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.AdminDashboard, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.ManagerDashboard, cancellationToken);

        return Result<bool>.Success(true, "Employee deactivated successfully");
    }
}
