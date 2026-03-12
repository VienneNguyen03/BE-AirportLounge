using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Employees.Commands;

public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public CreateEmployeeCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher,
        ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _unitOfWork.Users.Query()
            .AnyAsync(u => u.Email == request.Email, cancellationToken);

        var existingEmployeeCode = await _unitOfWork.Employees.Query()
            .AnyAsync(e => e.EmployeeCode == request.EmployeeCode, cancellationToken);

        if (existingUser || existingEmployeeCode)
            return Result<Guid>.Failure("A user with this email or employee code already exists");

        // Validate Department-Position relationship if both are provided
        if (request.DepartmentId.HasValue && request.PositionId.HasValue)
        {
            var position = await _unitOfWork.Positions.Query()
                .Include(p => p.Departments)
                .FirstOrDefaultAsync(p => p.Id == request.PositionId.Value, cancellationToken);

            if (position == null)
                return Result<Guid>.Failure("Position not found");

            if (!position.Departments.Any(d => d.Id == request.DepartmentId.Value))
                return Result<Guid>.Failure("The selected position does not belong to the selected department");
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            CreatedBy = _currentUser.Email
        };

        await _unitOfWork.Users.AddAsync(user, cancellationToken);

        var employee = new Employee
        {
            UserId = user.Id,
            EmployeeCode = request.EmployeeCode,
            DepartmentId = request.DepartmentId,
            PositionId = request.PositionId,
            Skills = request.Skills,
            HireDate = request.HireDate,
            DateOfBirth = request.DateOfBirth,
            NationalId = request.NationalId,
            Nationality = request.Nationality,
            Gender = request.Gender,
            MaritalStatus = request.MaritalStatus,
            PermanentAddress = request.PermanentAddress,
            TemporaryAddress = request.TemporaryAddress,
            TaxCode = request.TaxCode,
            BankAccountNumber = request.BankAccountNumber,
            BankName = request.BankName,
            BankAccountHolderName = request.BankAccountHolderName,
            BloodType = request.BloodType,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            EmergencyContactRelationship = request.EmergencyContactRelationship,
            ProfilePhotoUrl = request.ProfilePhotoUrl,
            CreatedBy = _currentUser.Email
        };

        await _unitOfWork.Employees.AddAsync(employee, cancellationToken);

        // Audit log
        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            EntityName = nameof(Employee),
            EntityId = employee.Id,
            Action = "Created",
            NewValues = System.Text.Json.JsonSerializer.Serialize(new { request.EmployeeCode, request.FullName, request.Email, request.Role }),
            PerformedBy = _currentUser.Email ?? "System",
            PerformedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate dashboard caches – employee count changed
        await _cache.RemoveAsync(CacheKeys.AdminDashboard, cancellationToken);
        await _cache.RemoveAsync(CacheKeys.ManagerDashboard, cancellationToken);

        return Result<Guid>.Success(employee.Id, "Employee created successfully");
    }
}
