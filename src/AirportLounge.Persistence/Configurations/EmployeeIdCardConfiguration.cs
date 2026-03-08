using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class EmployeeIdCardConfiguration : IEntityTypeConfiguration<EmployeeIdCard>
{
    public void Configure(EntityTypeBuilder<EmployeeIdCard> builder)
    {
        builder.ToTable("employee_id_cards");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.CardNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.TemplateType).HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<int>();
        builder.Property(e => e.RevokeReason).HasMaxLength(500);
        builder.Property(e => e.QrCodeData).HasMaxLength(2000);

        builder.HasIndex(e => e.CardNumber).IsUnique();

        builder.HasOne(e => e.Employee).WithMany(emp => emp.IdCards)
            .HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.IssuedBy).WithMany()
            .HasForeignKey(e => e.IssuedById).OnDelete(DeleteBehavior.Restrict);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
