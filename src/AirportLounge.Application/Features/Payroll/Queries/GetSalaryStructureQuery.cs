using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Payroll.Queries;

public record GetSalaryStructureQuery(Guid EmployeeId) : IRequest<Result<SalaryStructureDto>>;

public record SalaryStructureDto(
    Guid Id,
    Guid EmployeeId,
    decimal BaseSalary,
    decimal MealAllowance,
    decimal TransportAllowance,
    decimal NightShiftAllowance,
    decimal InsuranceDeduction,
    decimal TaxDeduction,
    DateTime EffectiveFrom);

public class GetSalaryStructureQueryHandler : IRequestHandler<GetSalaryStructureQuery, Result<SalaryStructureDto>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetSalaryStructureQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<SalaryStructureDto>> Handle(GetSalaryStructureQuery req, CancellationToken ct)
    {
        if (_currentUser.Role == "Staff")
        {
            var own = await _uow.Employees.Query()
                .Where(e => e.UserId == _currentUser.UserId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(ct);

            if (own == Guid.Empty || own != req.EmployeeId)
                return Result<SalaryStructureDto>.Failure("Staff can only view their own salary structure");
        }

        var cacheKey = CacheKeys.SalaryStructure(req.EmployeeId);
        var cached = await _cache.GetAsync<SalaryStructureDto>(cacheKey, ct);
        if (cached is not null)
            return Result<SalaryStructureDto>.Success(cached);

        var salary = await _uow.SalaryStructures.Query()
            .Where(s => s.EmployeeId == req.EmployeeId)
            .OrderByDescending(s => s.EffectiveFrom)
            .Select(s => new SalaryStructureDto(
                s.Id, s.EmployeeId, s.BaseSalary,
                s.MealAllowance, s.TransportAllowance, s.NightShiftAllowance,
                s.InsuranceDeduction, s.TaxDeduction, s.EffectiveFrom))
            .FirstOrDefaultAsync(ct);

        if (salary is null)
            return Result<SalaryStructureDto>.Failure("Salary structure not found");

        await _cache.SetAsync(cacheKey, salary, CacheKeys.SalaryTtl, ct);
        return Result<SalaryStructureDto>.Success(salary);
    }
}
