using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Application.Features.Leaves.Commands;

public record ConfigureLeaveBalanceCommand(
    Guid EmployeeId,
    Guid LeaveTypeId,
    int Year,
    decimal TotalDays) : IRequest<Result<Guid>>;

public class ConfigureLeaveBalanceCommandHandler : IRequestHandler<ConfigureLeaveBalanceCommand, Result<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public ConfigureLeaveBalanceCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ICacheService cache)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<Guid>> Handle(ConfigureLeaveBalanceCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.Role is not ("Admin" or "Manager"))
            return Result<Guid>.Failure("Only administrators and managers can configure leave balances");

        var employeeExists = await _unitOfWork.Employees.ExistsAsync(request.EmployeeId, cancellationToken);
        if (!employeeExists)
            return Result<Guid>.Failure("Employee not found");

        var leaveTypeExists = await _unitOfWork.LeaveTypes.ExistsAsync(request.LeaveTypeId, cancellationToken);
        if (!leaveTypeExists)
            return Result<Guid>.Failure("Leave type not found");

        var balance = await _unitOfWork.LeaveBalances.Query()
            .FirstOrDefaultAsync(lb =>
                lb.EmployeeId == request.EmployeeId &&
                lb.LeaveTypeId == request.LeaveTypeId &&
                lb.Year == request.Year &&
                !lb.IsDeleted, cancellationToken);

        if (balance is not null)
        {
            balance.TotalDays = request.TotalDays;
            balance.UpdatedBy = _currentUser.Email;
            _unitOfWork.LeaveBalances.Update(balance);
        }
        else
        {
            balance = new LeaveBalance
            {
                EmployeeId = request.EmployeeId,
                LeaveTypeId = request.LeaveTypeId,
                Year = request.Year,
                TotalDays = request.TotalDays,
                UsedDays = 0,
                CreatedBy = _currentUser.Email
            };
            await _unitOfWork.LeaveBalances.AddAsync(balance, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _cache.RemoveByPrefixAsync(CacheKeys.LeavesPrefix, cancellationToken);

        return Result<Guid>.Success(balance.Id, "Leave balance configured successfully");
    }
}
