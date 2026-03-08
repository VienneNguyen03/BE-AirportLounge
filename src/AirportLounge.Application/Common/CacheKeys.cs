namespace AirportLounge.Application.Common;

public static class CacheKeys
{
    // ── Dashboard ────────────────────────────────────────────────
    public const string AdminDashboard = "dashboard:admin";
    public const string ManagerDashboard = "dashboard:manager";
    public static string StaffDashboard(Guid employeeId) => $"dashboard:staff:{employeeId}";

    public static readonly TimeSpan DashboardTtl = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan StaffDashboardTtl = TimeSpan.FromMinutes(1);

    // ── Employees ────────────────────────────────────────────────
    public static string EmployeeById(Guid id) => $"employee:{id}";
    public static readonly TimeSpan EmployeeTtl = TimeSpan.FromMinutes(10);

    // ── Zones ────────────────────────────────────────────────────
    public static string ZonesList(string? status) => $"zones:list:{status ?? "all"}";
    public const string ZoneAlerts = "zones:alerts";
    public static readonly TimeSpan ZonesTtl = TimeSpan.FromMinutes(2);
    public static readonly TimeSpan ZoneAlertsTtl = TimeSpan.FromSeconds(30);

    public static readonly string[] AllZoneListKeys = new[]
    {
        ZonesList(null),
        ZonesList("Available"),
        ZonesList("Occupied"),
        ZonesList("NeedsCleaning"),
        ZonesList("NeedsSupport"),
        ZonesList("Full"),
        ZonesList("Closed"),
    };

    // ── V2: Leave Management ─────────────────────────────────────
    public const string LeaveTypesList = "leave:types";
    public static string LeaveBalance(Guid employeeId, int year) => $"leave:balance:{employeeId}:{year}";
    public static string LeaveRequests(Guid employeeId) => $"leave:requests:{employeeId}";
    public const string LeaveRequestsPending = "leave:requests:pending";
    public static readonly TimeSpan LeaveTypesTtl = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan LeaveBalanceTtl = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan LeaveRequestsTtl = TimeSpan.FromMinutes(2);

    // ── V2: Payroll ──────────────────────────────────────────────
    public static string PayrollByEmployee(Guid employeeId, int year) => $"payroll:{employeeId}:{year}";
    public static string SalaryStructure(Guid employeeId) => $"salary:{employeeId}";
    public static readonly TimeSpan PayrollTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan SalaryTtl = TimeSpan.FromMinutes(15);

    // ── V2: Performance & Training ───────────────────────────────
    public static string PerformanceGoals(Guid employeeId) => $"perf:goals:{employeeId}";
    public static string PerformanceReviews(Guid employeeId) => $"perf:reviews:{employeeId}";
    public const string TrainingCoursesList = "training:courses";
    public static string TrainingEnrollments(Guid employeeId) => $"training:enrollments:{employeeId}";
    public static readonly TimeSpan PerformanceTtl = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan TrainingTtl = TimeSpan.FromMinutes(10);

    // ── V2: Onboarding / Offboarding ─────────────────────────────
    public static string Onboarding(Guid employeeId) => $"onboarding:{employeeId}";
    public static string Offboarding(Guid employeeId) => $"offboarding:{employeeId}";
    public static readonly TimeSpan OnboardingTtl = TimeSpan.FromMinutes(5);

    // ── V2: ID Cards & Documents ─────────────────────────────────
    public static string IdCards(Guid employeeId) => $"idcards:{employeeId}";
    public static string Documents(Guid employeeId) => $"documents:{employeeId}";
    public static readonly TimeSpan IdCardsTtl = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan DocumentsTtl = TimeSpan.FromMinutes(5);
}
