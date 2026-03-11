using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class OffboardingProcessConfiguration : IEntityTypeConfiguration<OffboardingProcess>
{
    public void Configure(EntityTypeBuilder<OffboardingProcess> builder)
    {
        builder.ToTable("offboarding_processes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.Reason).HasMaxLength(1000);
        builder.Property(e => e.Notes).HasMaxLength(2000);
        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasOne(e => e.Employee).WithMany()
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
