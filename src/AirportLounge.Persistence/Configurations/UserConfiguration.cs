using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(u => u.EmployeeCode).IsUnique();

        builder.Property(u => u.FullName).HasMaxLength(200).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(200).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();

        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
        builder.Property(u => u.RefreshToken).HasMaxLength(500);

        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
