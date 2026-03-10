using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Persistence;

public static class DataSeeder
{
    public const string AdminEmail = "admin@airportlounge.com";
    public const string AdminDefaultPassword = "Admin@123";

    public static async Task SeedAdminAsync(AppDbContext context, CancellationToken ct = default)
    {
        var hasAdmin = await context.Users
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == AdminEmail, ct);

        if (hasAdmin)
            return;

        var admin = new User
        {
            Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            FullName = "System Administrator",
            Email = AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(AdminDefaultPassword, BCrypt.Net.BCrypt.GenerateSalt(12)),
            Role = UserRole.Admin,
            IsActive = true,
            CreatedBy = "System"
        };

        context.Users.Add(admin);
        await context.SaveChangesAsync(ct);
    }
}
