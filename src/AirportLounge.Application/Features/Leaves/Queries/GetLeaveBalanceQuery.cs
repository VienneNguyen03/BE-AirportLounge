using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Queries;

public record GetLeaveBalanceQuery(Guid EmployeeId, int Year) : IRequest<Result<List<LeaveBalanceDto>>>;

public record LeaveBalanceDto(
    Guid Id,
    Guid LeaveTypeId,
    string LeaveTypeName,
    int Year,
    decimal TotalDays,
    decimal UsedDays,
    decimal RemainingDays);

public class GetLeaveBalanceQueryHandler : IRequestHandler<GetLeaveBalanceQuery, Result<List<LeaveBalanceDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;

    public GetLeaveBalanceQueryHandler(IUnitOfWork unitOfWork, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _cache = cache;
    }

    public async Task<Result<List<LeaveBalanceDto>>> Handle(GetLeaveBalanceQuery request, CancellationToken cancellationToken)
    {
        var cacheKey = CacheKeys.LeaveBalance(request.EmployeeId, request.Year);
        var cached = await _cache.GetAsync<List<LeaveBalanceDto>>(cacheKey, cancellationToken);
        if (cached is not null)
            return Result<List<LeaveBalanceDto>>.Success(cached);

        var employeeExists = await _unitOfWork.Employees.ExistsAsync(request.EmployeeId, cancellationToken);
        if (!employeeExists)
            return Result<List<LeaveBalanceDto>>.Failure("Employee not found");

        var balances = await _unitOfWork.LeaveBalances.Query()
            .Include(lb => lb.LeaveType)
            .Where(lb => lb.EmployeeId == request.EmployeeId && lb.Year == request.Year && !lb.IsDeleted)
            .OrderBy(lb => lb.LeaveType.Name)
            .Select(lb => new LeaveBalanceDto(
                lb.Id,
                lb.LeaveTypeId,
                lb.LeaveType.Name,
                lb.Year,
                lb.TotalDays,
                lb.UsedDays,
                lb.RemainingDays))
            .ToListAsync(cancellationToken);

        await _cache.SetAsync(cacheKey, balances, CacheKeys.LeaveBalanceTtl, cancellationToken);

        return Result<List<LeaveBalanceDto>>.Success(balances);
    }
}
