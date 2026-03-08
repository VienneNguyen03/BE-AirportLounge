using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class TrainingEnrollmentConfiguration : IEntityTypeConfiguration<TrainingEnrollment>
{
    public void Configure(EntityTypeBuilder<TrainingEnrollment> builder)
    {
        builder.ToTable("training_enrollments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.Score).HasPrecision(5, 2);
        builder.Property(e => e.CertificateUrl).HasMaxLength(500);

        builder.HasIndex(e => new { e.EmployeeId, e.CourseId }).IsUnique();

        builder.HasOne(e => e.Employee).WithMany(emp => emp.TrainingEnrollments)
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Course).WithMany(c => c.Enrollments)
            .HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
