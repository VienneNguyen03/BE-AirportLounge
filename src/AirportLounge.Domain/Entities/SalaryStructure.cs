using AirportLounge.Domain.Common;

namespace AirportLounge.Domain.Entities;

public class SalaryStructure : AuditableEntity
{
    public Guid EmployeeId { get; set; }
    public decimal BaseSalary { get; set; }
    public decimal MealAllowance { get; set; }
    public decimal TransportAllowance { get; set; }
    public decimal NightShiftAllowance { get; set; }
    public decimal InsuranceDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public DateTime EffectiveFrom { get; set; }

    public Employee Employee { get; set; } = null!;
}
