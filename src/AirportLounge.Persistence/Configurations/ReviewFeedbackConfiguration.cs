using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class ReviewFeedbackConfiguration : IEntityTypeConfiguration<ReviewFeedback>
{
    public void Configure(EntityTypeBuilder<ReviewFeedback> builder)
    {
        builder.ToTable("review_feedbacks");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Comment).HasMaxLength(4000);
        builder.Property(e => e.Score).HasPrecision(5, 2);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
