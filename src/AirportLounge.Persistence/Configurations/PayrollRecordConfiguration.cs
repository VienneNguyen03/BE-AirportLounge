using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable("payroll_records");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BaseSalary).HasPrecision(18, 2);
        builder.Property(e => e.TotalAllowances).HasPrecision(18, 2);
        builder.Property(e => e.OvertimePay).HasPrecision(18, 2);
        builder.Property(e => e.Bonuses).HasPrecision(18, 2);
        builder.Property(e => e.TotalDeductions).HasPrecision(18, 2);
        builder.Property(e => e.NetSalary).HasPrecision(18, 2);
        builder.Property(e => e.WorkedHours).HasPrecision(8, 2);
        builder.Property(e => e.OvertimeHours).HasPrecision(8, 2);
        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.Notes).HasMaxLength(1000);

        builder.HasIndex(e => new { e.EmployeeId, e.Year, e.Month }).IsUnique();

        builder.HasOne(e => e.Employee).WithMany()
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
