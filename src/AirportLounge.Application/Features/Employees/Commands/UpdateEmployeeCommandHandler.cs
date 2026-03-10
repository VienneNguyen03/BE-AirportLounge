using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Employees.Commands;

public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, Result<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateEmployeeCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return Result<bool>.Failure("Employee not found");

        var oldValues = System.Text.Json.JsonSerializer.Serialize(new
        {
            employee.User.FullName, employee.User.PhoneNumber,
            employee.Department, employee.Position, employee.Skills
        });

        employee.User.FullName = request.FullName;
        employee.User.PhoneNumber = request.PhoneNumber;
        employee.User.UpdatedBy = _currentUser.Email;
        employee.Department = request.Department;
        employee.Position = request.Position;
        employee.Skills = request.Skills;
        if (request.HireDate.HasValue)
        {
            employee.HireDate = request.HireDate.Value;
        }
        employee.DateOfBirth = request.DateOfBirth;
        employee.NationalId = request.NationalId;
        employee.Nationality = request.Nationality;
        employee.Gender = request.Gender;
        employee.MaritalStatus = request.MaritalStatus;
        employee.PermanentAddress = request.PermanentAddress;
        employee.TemporaryAddress = request.TemporaryAddress;
        employee.TaxCode = request.TaxCode;
        employee.BankAccountNumber = request.BankAccountNumber;
        employee.BankName = request.BankName;
        employee.BankAccountHolderName = request.BankAccountHolderName;
        employee.BloodType = request.BloodType;
        employee.EmergencyContactName = request.EmergencyContactName;
        employee.EmergencyContactPhone = request.EmergencyContactPhone;
        employee.EmergencyContactRelationship = request.EmergencyContactRelationship;
        employee.ProfilePhotoUrl = request.ProfilePhotoUrl;
        employee.UpdatedBy = _currentUser.Email;

        _unitOfWork.Employees.Update(employee);

        await _unitOfWork.AuditLogs.AddAsync(new AuditLog
        {
            EntityName = nameof(Employee),
            EntityId = employee.Id,
            Action = "Updated",
            OldValues = oldValues,
            NewValues = System.Text.Json.JsonSerializer.Serialize(new
            {
                request.FullName, request.PhoneNumber,
                request.Department, request.Position, request.Skills
            }),
            PerformedBy = _currentUser.Email ?? "System",
            PerformedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate per-employee and dashboard caches
        await _cache.RemoveAsync(CacheKeys.EmployeeById(request.EmployeeId), cancellationToken);
        await _cache.RemoveAsync(CacheKeys.StaffDashboard(request.EmployeeId), cancellationToken);

        return Result<bool>.Success(true, "Employee updated successfully");
    }
}
