using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class TrainingCourseConfiguration : IEntityTypeConfiguration<TrainingCourse>
{
    public void Configure(EntityTypeBuilder<TrainingCourse> builder)
    {
        builder.ToTable("training_courses");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.Property(e => e.Category).HasMaxLength(100);
        builder.Property(e => e.ContentUrl).HasMaxLength(500);
        builder.Property(e => e.PassingScore).HasPrecision(5, 2);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
