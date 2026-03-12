using AirportLounge.Domain.Entities;
using AirportLounge.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirportLounge.Persistence;

public static class DataSeeder
{
    public const string AdminEmail = "admin@airportlounge.com";
    public const string AdminDefaultPassword = "Admin@123";
    public const string StaffDefaultPassword = "Staff@123";

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

    /// <summary>
    /// Seed demo staff accounts for testing (users + employees).
    /// Creates up to 300 staff with codes STF001..STF300 if they don't already exist.
    /// </summary>
    public static async Task SeedDemoStaffAsync(AppDbContext context, CancellationToken ct = default)
    {
        // Nếu đã có >= 300 nhân viên (bất kỳ) thì bỏ qua seeding demo để tránh phình dữ liệu quá nhiều
        var existingEmployees = await context.Employees
            .IgnoreQueryFilters()
            .CountAsync(ct);
        if (existingEmployees >= 300) return;

        var usersToAdd = new List<User>();
        var employeesToAdd = new List<Employee>();

        for (var i = 1; i <= 300; i++)
        {
            var code = $"STF{i:000}";
            var email = $"staff{i:000}@airportlounge.com";

            // Bỏ qua nếu đã tồn tại user/email này
            var existsUser = await context.Users
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email == email, ct);
            if (existsUser) continue;

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = $"Staff {i:000}",
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(StaffDefaultPassword, BCrypt.Net.BCrypt.GenerateSalt(12)),
                Role = UserRole.Staff,
                IsActive = true,
                CreatedBy = "DataSeeder"
            };

            var employee = new Employee
            {
                UserId = user.Id,
                EmployeeCode = code,
                HireDate = DateTime.UtcNow.Date,
                Nationality = "VN",
                Gender = Gender.Male,
                IsDeleted = false,
                CreatedBy = "DataSeeder"
            };

            usersToAdd.Add(user);
            employeesToAdd.Add(employee);
        }

        if (usersToAdd.Count == 0) return;

        await context.Users.AddRangeAsync(usersToAdd, ct);
        await context.Employees.AddRangeAsync(employeesToAdd, ct);
        await context.SaveChangesAsync(ct);
    }
}
