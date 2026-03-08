using AirportLounge.Domain.Common;
using AirportLounge.Domain.Enums;

namespace AirportLounge.Domain.Entities;

public class PayrollRecord : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal TotalAllowances { get; set; }
    public decimal OvertimePay { get; set; }
    public decimal Bonuses { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetSalary { get; set; }
    public decimal WorkedHours { get; set; }
    public decimal OvertimeHours { get; set; }
    public int UnpaidLeaveDays { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTime? PaidAt { get; set; }
    public string? Notes { get; set; }

    public Employee Employee { get; set; } = null!;
}
