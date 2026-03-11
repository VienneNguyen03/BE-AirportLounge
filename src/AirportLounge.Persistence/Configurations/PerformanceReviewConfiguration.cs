using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class PerformanceReviewConfiguration : IEntityTypeConfiguration<PerformanceReview>
{
    public void Configure(EntityTypeBuilder<PerformanceReview> builder)
    {
        builder.ToTable("performance_reviews");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Period).IsRequired().HasMaxLength(50);
        builder.Property(e => e.ReviewType).HasConversion<int>();
        builder.Property(e => e.SelfAssessment).HasMaxLength(4000);
        builder.Property(e => e.ManagerAssessment).HasMaxLength(4000);
        builder.Property(e => e.SelfScore).HasPrecision(5, 2);
        builder.Property(e => e.ManagerScore).HasPrecision(5, 2);
        builder.Property(e => e.OverallScore).HasPrecision(5, 2);
        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.Comments).HasMaxLength(2000);
        builder.Property(e => e.ImprovementPlan).HasMaxLength(4000);
        builder.Property(e => e.RowVersion)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate();

        builder.HasOne(e => e.Employee).WithMany(emp => emp.PerformanceReviews)
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Reviewer).WithMany()
            .HasForeignKey(e => e.ReviewerId).OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.Feedbacks).WithOne(f => f.Review)
            .HasForeignKey(f => f.ReviewId).OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
