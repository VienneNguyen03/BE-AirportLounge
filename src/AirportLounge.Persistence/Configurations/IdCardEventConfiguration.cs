using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class IdCardEventConfiguration : IEntityTypeConfiguration<IdCardEvent>
{
    public void Configure(EntityTypeBuilder<IdCardEvent> builder)
    {
        builder.ToTable("id_card_events");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FromStatus).HasConversion<int>();
        builder.Property(e => e.ToStatus).HasConversion<int>();
        builder.Property(e => e.Action).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Comment).HasMaxLength(1000);

        builder.HasOne(e => e.Card).WithMany(c => c.Events)
            .HasForeignKey(e => e.CardId).OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.PerformedBy).WithMany()
            .HasForeignKey(e => e.PerformedById).OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
