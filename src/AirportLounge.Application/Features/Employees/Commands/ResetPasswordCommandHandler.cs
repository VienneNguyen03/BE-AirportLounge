using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Employees.Commands;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
    }

    public async Task<Result<bool>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee == null)
            return Result<bool>.Failure("Employee not found");

        if (employee.User == null)
            return Result<bool>.Failure("User account not found for this employee");

        employee.User.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        // Audit log
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            EntityName = nameof(User),
            EntityId = employee.UserId,
            Action = "ResetPassword",
            NewValues = "Password has been reset by administrator",
            PerformedBy = _currentUser.Email ?? "System",
            PerformedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true, "Password reset successfully");
    }
}
