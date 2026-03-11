using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_requests");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.TotalDays).HasPrecision(5, 1);
        builder.Property(e => e.Reason).HasMaxLength(1000);
        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.ReviewerComment).HasMaxLength(1000);
        builder.Property(e => e.DecisionReason).HasMaxLength(1000);

        // Optimistic concurrency
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(e => e.Employee).WithMany(emp => emp.LeaveRequests)
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.LeaveType).WithMany(lt => lt.LeaveRequests)
            .HasForeignKey(e => e.LeaveTypeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.ReviewedBy).WithMany()
            .HasForeignKey(e => e.ReviewedById).OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(e => e.Attachments).WithOne(a => a.LeaveRequest)
            .HasForeignKey(a => a.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
