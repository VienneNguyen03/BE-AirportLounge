using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Employees.Queries;

public record GetEmployeeByIdQuery(Guid EmployeeId) : IRequest<Result<EmployeeDetailDto>>;

public record EmployeeDetailDto(
    Guid Id, Guid UserId, string EmployeeCode, string FullName, string Email,
    string? PhoneNumber, string Role, string? Department, string? Position,
    string? Skills, string? Address, DateTime? DateOfBirth, DateTime HireDate,
    bool IsActive, DateTime CreatedAt,
    string? NationalId, string? Nationality, string? Gender, string? MaritalStatus,
    string? PermanentAddress, string? TemporaryAddress, string? TaxCode,
    string? BankAccountNumber, string? BankName, string? BloodType,
    string? EmergencyContactName, string? EmergencyContactPhone,
    string? EmergencyContactRelationship, string? ProfilePhotoUrl);

public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, Result<EmployeeDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetEmployeeByIdQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<EmployeeDetailDto>> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.EmployeeById(request.EmployeeId);
        var cached = await _cache.GetAsync<EmployeeDetailDto>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<EmployeeDetailDto>.Success(cached);

        var employee = await _unitOfWork.Employees.Query()
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken);

        if (employee is null)
            return Result<EmployeeDetailDto>.Failure("Employee not found");

        var dto = new EmployeeDetailDto(
            employee.Id, employee.UserId, employee.User.EmployeeCode,
            employee.User.FullName, employee.User.Email, employee.User.PhoneNumber,
            employee.User.Role.ToString(), employee.Department, employee.Position,
            employee.Skills, employee.Address, employee.DateOfBirth, employee.HireDate,
            employee.User.IsActive, employee.CreatedAt,
            employee.NationalId, employee.Nationality,
            employee.Gender?.ToString(), employee.MaritalStatus?.ToString(),
            employee.PermanentAddress, employee.TemporaryAddress, employee.TaxCode,
            employee.BankAccountNumber, employee.BankName, employee.BloodType,
            employee.EmergencyContactName, employee.EmergencyContactPhone,
            employee.EmergencyContactRelationship, employee.ProfilePhotoUrl);

        await _cache.SetAsync(cacheKey, dto, CacheKeys.EmployeeTtl, cancellationToken);
        return Result<EmployeeDetailDto>.Success(dto);
    }
}
