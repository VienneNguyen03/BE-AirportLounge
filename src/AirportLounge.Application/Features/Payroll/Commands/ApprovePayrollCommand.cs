using AirportLounge.Application.Common;
using AirportLounge.Application.Common.Interfaces;
using AirportLounge.Application.Common.Models;
using AirportLounge.Domain.Enums;
using AirportLounge.Domain.Interfaces;
using MediatR;

namespace AirportLounge.Application.Features.Payroll.Commands;

public record ApprovePayrollCommand(Guid PayrollRecordId) : IRequest<Result<bool>>;

public class ApprovePayrollCommandHandler : IRequestHandler<ApprovePayrollCommand, Result<bool>>
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public ApprovePayrollCommandHandler(IUnitOfWork uow, ICurrentUserService currentUser, ICacheService cache)
    {
        _uow = uow;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<Result<bool>> Handle(ApprovePayrollCommand req, CancellationToken ct)
    {
        if (_currentUser.Role != "Admin")
            return Result<bool>.Failure("Only Admin can approve or mark payroll as paid");

        var record = await _uow.PayrollRecords.GetByIdAsync(req.PayrollRecordId, ct);
        if (record is null)
            return Result<bool>.Failure("Payroll record not found");

        switch (record.Status)
        {
            case PayrollStatus.Draft:
                record.Status = PayrollStatus.Approved;
                record.UpdatedBy = _currentUser.Email;
                break;

            case PayrollStatus.Approved:
                record.Status = PayrollStatus.Paid;
                record.PaidAt = DateTime.UtcNow;
                record.UpdatedBy = _currentUser.Email;
                break;

            default:
                return Result<bool>.Failure(
                    $"Cannot transition from {record.Status}. Expected Draft or Approved.");
        }

        _uow.PayrollRecords.Update(record);
        await _uow.SaveChangesAsync(ct);

        await _cache.RemoveAsync(CacheKeys.PayrollByEmployee(record.EmployeeId, record.Year), ct);

        return Result<bool>.Success(true, $"Payroll status updated to {record.Status}");
    }
}
