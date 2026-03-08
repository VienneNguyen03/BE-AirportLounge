using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using MediatR;

namespace AirportLounge.Application.Features.Employees.Commands;

public record CreateEmployeeCommand(
    string EmployeeCode,
    string FullName,
    string Email,
    string? PhoneNumber,
    string Password,
    UserRole Role,
    string? Department,
    string? Position,
    string? Skills,
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
    string? BloodType,
    string? EmergencyContactName,
    string? EmergencyContactPhone,
    string? EmergencyContactRelationship
) : IRequest<Result<Guid>>;
