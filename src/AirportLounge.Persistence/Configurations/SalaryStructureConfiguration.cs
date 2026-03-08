using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class SalaryStructureConfiguration : IEntityTypeConfiguration<SalaryStructure>
{
    public void Configure(EntityTypeBuilder<SalaryStructure> builder)
    {
        builder.ToTable("salary_structures");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.BaseSalary).HasPrecision(18, 2);
        builder.Property(e => e.MealAllowance).HasPrecision(18, 2);
        builder.Property(e => e.TransportAllowance).HasPrecision(18, 2);
        builder.Property(e => e.NightShiftAllowance).HasPrecision(18, 2);
        builder.Property(e => e.InsuranceDeduction).HasPrecision(18, 2);
        builder.Property(e => e.TaxDeduction).HasPrecision(18, 2);

        builder.HasOne(e => e.Employee).WithOne(emp => emp.SalaryStructure)
            .HasForeignKey<SalaryStructure>(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
