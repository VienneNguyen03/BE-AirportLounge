using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class LoungeZoneConfiguration : IEntityTypeConfiguration<LoungeZone>
{
    public void Configure(EntityTypeBuilder<LoungeZone> builder)
    {
        builder.ToTable("lounge_zones");
        builder.HasKey(z => z.Id);

        builder.Property(z => z.Name).HasMaxLength(200).IsRequired();
        builder.Property(z => z.Description).HasMaxLength(500);
        builder.Property(z => z.Status).HasConversion<string>().HasMaxLength(30);

        builder.HasQueryFilter(z => !z.IsDeleted);
    }
}
