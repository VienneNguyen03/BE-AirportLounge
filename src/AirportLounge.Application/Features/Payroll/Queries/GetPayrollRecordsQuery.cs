using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Payroll.Queries;

public record GetPayrollRecordsQuery(Guid EmployeeId, int Year) : IRequest<Result<List<PayrollRecordDto>>>;

public record PayrollRecordDto(
    Guid Id,
    Guid EmployeeId,
    int Year,
    int Month,
    decimal BaseSalary,
    decimal TotalAllowances,
    decimal OvertimePay,
    decimal Bonuses,
    decimal TotalDeductions,
    decimal NetSalary,
    decimal WorkedHours,
    decimal OvertimeHours,
    int UnpaidLeaveDays,
    PayrollStatus Status,
    DateTime? PaidAt,
    string? Notes);

public class GetPayrollRecordsQueryHandler : IRequestHandler<GetPayrollRecordsQuery, Result<List<PayrollRecordDto>>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public GetPayrollRecordsQueryHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<List<PayrollRecordDto>>> Handle(GetPayrollRecordsQuery req, CancellationToken ct)
    {
        if (_currentUser.Role == "Staff")
        {
            var own = await _uow.Employees.Query()
                .Where(e => e.UserId == _currentUser.UserId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync(ct);

            if (own == Guid.Empty || own != req.EmployeeId)
                return Result<List<PayrollRecordDto>>.Failure("Staff can only view their own payroll records");
        }

        var cacheKey = CacheKeys.PayrollByEmployee(req.EmployeeId, req.Year);
        var cached = await _cache.GetAsync<List<PayrollRecordDto>>(cacheKey, ct);
        if (cached is not null)
            return Result<List<PayrollRecordDto>>.Success(cached);

        var records = await _uow.PayrollRecords.Query()
            .Where(p => p.EmployeeId == req.EmployeeId && p.Year == req.Year)
            .OrderBy(p => p.Month)
            .Select(p => new PayrollRecordDto(
                p.Id, p.EmployeeId, p.Year, p.Month,
                p.BaseSalary, p.TotalAllowances, p.OvertimePay, p.Bonuses,
                p.TotalDeductions, p.NetSalary, p.WorkedHours, p.OvertimeHours,
                p.UnpaidLeaveDays, p.Status, p.PaidAt, p.Notes))
            .ToListAsync(ct);

        await _cache.SetAsync(cacheKey, records, CacheKeys.PayrollTtl, ct);
        return Result<List<PayrollRecordDto>>.Success(records);
    }
}
