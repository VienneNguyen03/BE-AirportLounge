using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class LeaveBalanceConfiguration : IEntityTypeConfiguration<LeaveBalance>
{
    public void Configure(EntityTypeBuilder<LeaveBalance> builder)
    {
        builder.ToTable("leave_balances");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TotalDays).HasPrecision(5, 1);
        builder.Property(e => e.UsedDays).HasPrecision(5, 1);
        builder.Ignore(e => e.RemainingDays);

        builder.HasIndex(e => new { e.EmployeeId, e.LeaveTypeId, e.Year }).IsUnique();

        builder.HasOne(e => e.Employee).WithMany(emp => emp.LeaveBalances)
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.LeaveType).WithMany(lt => lt.LeaveBalances)
            .HasForeignKey(e => e.LeaveTypeId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
