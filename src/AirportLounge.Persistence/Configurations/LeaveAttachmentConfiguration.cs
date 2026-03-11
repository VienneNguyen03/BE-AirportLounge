using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class LeaveAttachmentConfiguration : IEntityTypeConfiguration<LeaveAttachment>
{
    public void Configure(EntityTypeBuilder<LeaveAttachment> builder)
    {
        builder.ToTable("leave_attachments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FileUrl).HasMaxLength(500).IsRequired();
        builder.Property(e => e.FileName).HasMaxLength(255).IsRequired();

        builder.HasOne(e => e.LeaveRequest).WithMany(lr => lr.Attachments)
            .HasForeignKey(e => e.LeaveRequestId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
