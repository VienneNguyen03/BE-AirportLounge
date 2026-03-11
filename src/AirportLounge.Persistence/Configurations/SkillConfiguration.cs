using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("skills");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.Name).IsUnique().HasFilter("\"IsDeleted\" = false");
        
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
