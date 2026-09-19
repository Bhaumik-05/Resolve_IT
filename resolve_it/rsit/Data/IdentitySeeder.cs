using Microsoft.AspNetCore.Identity;
using rsit.Models;

namespace rsit.Data;

public static class IdentitySeeder
{
    public static async Task SeedRolesAsync(RoleManager<IdentityRole<int>> roleManager)
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
}