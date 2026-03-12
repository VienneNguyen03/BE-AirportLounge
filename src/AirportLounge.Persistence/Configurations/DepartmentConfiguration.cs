using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("departments");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(e => e.Name).IsUnique().HasFilter("\"IsDeleted\" = false");
        
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasQueryFilter(e => !e.IsDeleted);

        builder
            .HasMany(d => d.Positions)
            .WithMany(p => p.Departments)
            .UsingEntity(j => j.ToTable("department_positions"));
    }
}
