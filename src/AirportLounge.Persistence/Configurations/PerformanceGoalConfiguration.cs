using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class PerformanceGoalConfiguration : IEntityTypeConfiguration<PerformanceGoal>
{
    public void Configure(EntityTypeBuilder<PerformanceGoal> builder)
    {
        builder.ToTable("performance_goals");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);
        builder.Property(e => e.TargetValue).HasPrecision(18, 2);
        builder.Property(e => e.CurrentValue).HasPrecision(18, 2);
        builder.Property(e => e.Unit).HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<int>();

        builder.HasOne(e => e.Employee).WithMany(emp => emp.PerformanceGoals)
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
