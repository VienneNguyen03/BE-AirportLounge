using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class ShiftAssignmentConfiguration : IEntityTypeConfiguration<ShiftAssignment>
{
    public void Configure(EntityTypeBuilder<ShiftAssignment> builder)
    {
        builder.ToTable("shift_assignments");
        builder.HasKey(sa => sa.Id);

        builder.Property(sa => sa.Notes).HasMaxLength(500);

        builder.HasOne(sa => sa.Shift)
            .WithMany(s => s.ShiftAssignments)
            .HasForeignKey(sa => sa.ShiftId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sa => sa.Employee)
            .WithMany(e => e.ShiftAssignments)
            .HasForeignKey(sa => sa.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sa => sa.LoungeZone)
            .WithMany(z => z.ShiftAssignments)
            .HasForeignKey(sa => sa.LoungeZoneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Prevent double-booking: unique on Employee + Date + Shift
        builder.HasIndex(sa => new { sa.EmployeeId, sa.Date, sa.ShiftId }).IsUnique();

        builder.HasQueryFilter(sa => !sa.IsDeleted);
    }
}
