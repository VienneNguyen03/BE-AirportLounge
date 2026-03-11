using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

namespace AirportLounge.Application.Features.Employees.Commands;

public record UpdateEmployeeCommand(
    Guid EmployeeId,
    string FullName,
    string? PhoneNumber,
    Guid? DepartmentId,
    Guid? PositionId,
    string? Skills,
    DateTime? HireDate,
    DateTime? DateOfBirth,
    string? NationalId,
    string? Nationality,
    Gender? Gender,
    MaritalStatus? MaritalStatus,
    string? PermanentAddress,
    string? TemporaryAddress,
    string? TaxCode,
    string? BankAccountNumber,
    string? BankName,
    string? BankAccountHolderName,
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelationship,
    string? ProfilePhotoUrl
) : IRequest<Result<bool>>;
