using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class OnboardingProcessConfiguration : IEntityTypeConfiguration<OnboardingProcess>
{
    public void Configure(EntityTypeBuilder<OnboardingProcess> builder)
    {
        builder.ToTable("onboarding_processes");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<int>();

        builder.HasOne(e => e.Employee).WithMany()
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.AssignedMentor).WithMany()
            .HasForeignKey(e => e.AssignedMentorId).OnDelete(DeleteBehavior.SetNull);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
