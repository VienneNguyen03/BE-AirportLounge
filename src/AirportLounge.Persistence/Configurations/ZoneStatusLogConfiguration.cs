using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class ZoneStatusLogConfiguration : IEntityTypeConfiguration<ZoneStatusLog>
{
    public void Configure(EntityTypeBuilder<ZoneStatusLog> builder)
    {
        builder.ToTable("zone_status_logs");
        builder.HasKey(z => z.Id);

        builder.Property(z => z.PreviousStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(z => z.NewStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(z => z.ChangedBy).HasMaxLength(200);
        builder.Property(z => z.Notes).HasMaxLength(500);

        builder.HasOne(z => z.LoungeZone)
            .WithMany(lz => lz.StatusLogs)
            .HasForeignKey(z => z.LoungeZoneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
