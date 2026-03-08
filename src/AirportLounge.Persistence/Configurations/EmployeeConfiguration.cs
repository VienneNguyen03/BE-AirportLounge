using AirportLounge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AirportLounge.Persistence.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("employees");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Department).HasMaxLength(100);
        builder.Property(e => e.Position).HasMaxLength(100);
        builder.Property(e => e.Skills).HasMaxLength(500);
        builder.Property(e => e.Address).HasMaxLength(500);

        builder.Property(e => e.NationalId).HasMaxLength(50);
        builder.Property(e => e.Nationality).HasMaxLength(100);
        builder.Property(e => e.Gender).HasConversion<int>();
        builder.Property(e => e.MaritalStatus).HasConversion<int>();
        builder.Property(e => e.PermanentAddress).HasMaxLength(500);
        builder.Property(e => e.TemporaryAddress).HasMaxLength(500);
        builder.Property(e => e.TaxCode).HasMaxLength(50);
        builder.Property(e => e.BankAccountNumber).HasMaxLength(50);
        builder.Property(e => e.BankName).HasMaxLength(200);
        builder.Property(e => e.BloodType).HasMaxLength(10);
        builder.Property(e => e.EmergencyContactName).HasMaxLength(200);
        builder.Property(e => e.EmergencyContactPhone).HasMaxLength(20);
        builder.Property(e => e.EmergencyContactRelationship).HasMaxLength(100);
        builder.Property(e => e.ProfilePhotoUrl).HasMaxLength(500);

        builder.HasOne(e => e.User)
            .WithOne(u => u.Employee)
            .HasForeignKey<Employee>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
