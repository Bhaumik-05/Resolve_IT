namespace rsit.Models;

public static class UserRoles
{
    public const string Employee = "Employee";
    public const string Admin = "Admin";
    public const string SupportStaff = "SupportStaff";

    public static readonly string[] All = { Employee, SupportStaff, Admin };

    public static bool IsValid(string? role) => role != null && All.Contains(role);

    /// <summary>Prefix used when auto-generating an Employee ID.</summary>
    public static string Prefix(string role) => role switch
    {
        Admin => "ADM",
        SupportStaff => "STF",
        _ => "EMP"
    };

    public static string DisplayName(string? role) =>
        role == SupportStaff ? "Support Staff" : role ?? "-";
}
