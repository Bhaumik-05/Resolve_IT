using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using rsit.Models;

namespace rsit.Data;

public static class IdentitySeeder
{
    // =========================
    // SEED ROLES
    // =========================
    public static async Task SeedRolesAsync(
        RoleManager<IdentityRole<int>> roleManager)
    {
        string[] roles =
        {
            UserRoles.Employee,
            UserRoles.Admin,
            UserRoles.SupportStaff
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(
                    new IdentityRole<int>(role));

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description));

                    throw new Exception(
                        $"Failed to create role '{role}': {errors}");
                }
            }
        }
    }

    // =========================
    // SEED DEPARTMENTS
    // =========================
    public static async Task SeedDepartmentsAsync(
        ApplicationDbContext context)
    {
        if (await context.Departments.AnyAsync())
            return;

        var departments = new List<Department>
        {
            new Department
            {
                Name = "Information Technology",
                Description = "Software development and IT services.",
                Status = "Active"
            },

            new Department
            {
                Name = "Human Resources",
                Description = "Employee management and recruitment.",
                Status = "Active"
            },

            new Department
            {
                Name = "Finance",
                Description = "Accounting and financial management.",
                Status = "Active"
            },

            new Department
            {
                Name = "Operations",
                Description = "Business operations and process management.",
                Status = "Active"
            },

            new Department
            {
                Name = "Customer Support",
                Description = "Customer complaints and technical support.",
                Status = "Active"
            }
        };

        await context.Departments.AddRangeAsync(departments);
        await context.SaveChangesAsync();
    }

    // =========================
    // SEED USERS
    // =========================
    public static async Task SeedUsersAsync(
        UserManager<User> userManager,
        ApplicationDbContext context)
    {
        var itDepartment = await context.Departments
            .FirstAsync(d => d.Name == "Information Technology");

        var hrDepartment = await context.Departments
            .FirstAsync(d => d.Name == "Human Resources");

        var supportDepartment = await context.Departments
            .FirstAsync(d => d.Name == "Customer Support");


        // =========================
        // ADMIN
        // =========================
        await CreateUserAsync(
            userManager,
            email: "admin@resolveit.com",
            password: "Admin@123",
            employeeId: "EMP001",
            name: "System Admin",
            accountStatus: "Active",
            departmentId: itDepartment.DepartmentId,
            role: UserRoles.Admin
        );


        // =========================
        // SUPPORT STAFF
        // =========================
        await CreateUserAsync(
            userManager,
            email: "staff@resolveit.com",
            password: "Staff@123",
            employeeId: "EMP002",
            name: "Support Staff",
            accountStatus: "Active",
            departmentId: supportDepartment.DepartmentId,
            role: UserRoles.SupportStaff
        );


        // =========================
        // EMPLOYEE
        // =========================
        await CreateUserAsync(
            userManager,
            email: "employee@resolveit.com",
            password: "Employee@123",
            employeeId: "EMP003",
            name: "Test Employee",
            accountStatus: "Active",
            departmentId: hrDepartment.DepartmentId,
            role: UserRoles.Employee
        );
    }

    // =========================
    // CREATE USER
    // =========================
    private static async Task CreateUserAsync(
        UserManager<User> userManager,
        string email,
        string password,
        string employeeId,
        string name,
        string accountStatus,
        int departmentId,
        string role)
    {
        var existingUser =
            await userManager.FindByEmailAsync(email);

        if (existingUser != null)
            return;

        var user = new User
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,

            EmployeeId = employeeId,
            Name = name,
            AccountStatus = accountStatus,
            CreatedAt = DateTime.UtcNow,

            DepartmentId = departmentId
        };

        var result =
            await userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description));

            throw new Exception(
                $"Failed to create user '{email}': {errors}");
        }

        var roleResult =
            await userManager.AddToRoleAsync(user, role);

        if (!roleResult.Succeeded)
        {
            var errors = string.Join(
                ", ",
                roleResult.Errors.Select(e => e.Description));

            throw new Exception(
                $"Failed to assign role '{role}' to '{email}': {errors}");
        }
    }
}