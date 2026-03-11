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

        builder.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.EmployeeCode)
            .IsUnique()
            .HasFilter("\"IsDeleted\" = false");

        builder.HasOne(e => e.Department)
            .WithMany(d => d.Employees)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(e => e.Position)
            .WithMany(p => p.Employees)
            .HasForeignKey(e => e.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(e => e.Skills).HasMaxLength(1000);

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
