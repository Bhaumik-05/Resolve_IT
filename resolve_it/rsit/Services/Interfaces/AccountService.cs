using Microsoft.AspNetCore.Identity;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels;

namespace rsit.Services;

public class AccountService : IAccountService
{
    private const string EmployeeRole = "Employee";

    private readonly IDepartmentRepository _departmentRepository;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly RoleManager<IdentityRole<int>> _roleManager;
    private readonly IUserRepository _userRepository;

    public AccountService(
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        RoleManager<IdentityRole<int>> roleManager,
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
    }

    // =========================================================
    // REGISTER
    // =========================================================

    public async Task<ServiceResult> RegisterAsync(
        RegisterViewModel model)
    {
        // Normalize input
        var email = model.Email.Trim().ToLowerInvariant();
        var employeeId = model.EmployeeId.Trim();
        var name = model.Name.Trim();
        var mobile = model.Mobile.Trim();

        // -----------------------------------------------------
        // 1. Check duplicate email
        // -----------------------------------------------------

        var existingEmail =
            await _userManager.FindByEmailAsync(email);

        if (existingEmail != null)
        {
            return ServiceResult.Failure(
                "An account with this email already exists.");
        }

        // -----------------------------------------------------
        // 2. Check duplicate employee ID
        // -----------------------------------------------------

        var employeeExists =
            await _userRepository.EmployeeIdExistsAsync(employeeId);

        if (employeeExists)
        {
            return ServiceResult.Failure(
                "An account with this employee ID already exists.");
        }

        // -----------------------------------------------------
        // 3. Check department exists
        // -----------------------------------------------------

        var departmentExists =
            await _departmentRepository.ExistsAsync(
                model.DepartmentId);

        if (!departmentExists)
        {
            return ServiceResult.Failure(
                "The selected department does not exist.");
        }

        // -----------------------------------------------------
        // 4. Create Identity User
        // -----------------------------------------------------

        var user = new User
        {
            UserName = email,
            Email = email,

            Name = name,
            EmployeeId = employeeId,

            PhoneNumber = mobile,

            DepartmentId = model.DepartmentId,

            AccountStatus = "Active",
            CreatedAt = DateTime.UtcNow
        };

        // -----------------------------------------------------
        // 5. Create user using ASP.NET Core Identity
        // -----------------------------------------------------

        var identityResult =
            await _userManager.CreateAsync(
                user,
                model.Password);

        if (!identityResult.Succeeded)
        {
            var errors = identityResult.Errors
                .Select(error => error.Description)
                .ToArray();

            return ServiceResult.Failure(errors);
        }

        // -----------------------------------------------------
        // 6. Make sure Employee role exists
        // -----------------------------------------------------

        if (!await _roleManager.RoleExistsAsync(EmployeeRole))
        {
            var roleResult =
                await _roleManager.CreateAsync(
                    new IdentityRole<int>(EmployeeRole));

            if (!roleResult.Succeeded)
            {
                // User was created but role creation failed.
                // Remove the incomplete account.
                await _userManager.DeleteAsync(user);

                var errors = roleResult.Errors
                    .Select(error => error.Description)
                    .ToArray();

                return ServiceResult.Failure(errors);
            }
        }

        // -----------------------------------------------------
        // 7. Assign Employee role
        // -----------------------------------------------------

        var roleAssignmentResult =
            await _userManager.AddToRoleAsync(
                user,
                EmployeeRole);

        if (!roleAssignmentResult.Succeeded)
        {
            // Remove incomplete account
            await _userManager.DeleteAsync(user);

            var errors = roleAssignmentResult.Errors
                .Select(error => error.Description)
                .ToArray();

            return ServiceResult.Failure(errors);
        }

        // -----------------------------------------------------
        // 8. Registration successful
        // -----------------------------------------------------

        return ServiceResult.Success();
    }


    // =========================================================
    // LOGIN
    // =========================================================

    public async Task<ServiceResult> LoginAsync(
        LoginViewModel model)
    {
        // Normalize email
        var email = model.Email.Trim().ToLowerInvariant();

        // -----------------------------------------------------
        // 1. Find user
        // -----------------------------------------------------

        var user =
            await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Don't reveal whether the email exists.
            return ServiceResult.Failure(
                "Invalid email or password.");
        }

        // -----------------------------------------------------
        // 2. Check account status
        // -----------------------------------------------------

        if (!string.Equals(
                user.AccountStatus,
                "Active",
                StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult.Failure(
                "Your account is inactive. Please contact the administrator.");
        }

        // -----------------------------------------------------
        // 3. Authenticate using ASP.NET Core Identity
        // -----------------------------------------------------

        var result =
            await _signInManager.PasswordSignInAsync(
                user.UserName!,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: true);

        // -----------------------------------------------------
        // 4. Successful login
        // -----------------------------------------------------

        if (result.Succeeded)
        {
            return ServiceResult.Success();
        }

        // -----------------------------------------------------
        // 5. Account locked
        // -----------------------------------------------------

        if (result.IsLockedOut)
        {
            return ServiceResult.Failure(
                "Your account has been temporarily locked due to multiple failed login attempts.");
        }

        // -----------------------------------------------------
        // 6. Login not allowed
        // -----------------------------------------------------

        if (result.IsNotAllowed)
        {
            return ServiceResult.Failure(
                "Login is currently not allowed for this account.");
        }

        // -----------------------------------------------------
        // 7. Invalid credentials
        // -----------------------------------------------------

        return ServiceResult.Failure(
            "Invalid email or password.");
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}