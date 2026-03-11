using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class OffboardingTaskConfiguration : IEntityTypeConfiguration<OffboardingTask>
{
    public void Configure(EntityTypeBuilder<OffboardingTask> builder)
    {
        builder.ToTable("offboarding_tasks");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Category).HasMaxLength(100);

        builder.HasOne(e => e.Process).WithMany(p => p.Tasks)
            .HasForeignKey(e => e.ProcessId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
