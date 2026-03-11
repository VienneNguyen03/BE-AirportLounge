using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class IdCardTemplateConfiguration : IEntityTypeConfiguration<IdCardTemplate>
{
    public void Configure(EntityTypeBuilder<IdCardTemplate> builder)
    {
        builder.ToTable("id_card_templates");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description).HasMaxLength(500);
        builder.Property(e => e.LayoutJson).HasColumnType("jsonb").IsRequired();

        builder.HasIndex(e => e.Name).IsUnique();

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
