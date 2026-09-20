using Microsoft.AspNetCore.Identity;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels;

namespace rsit.Services;

public class AccountService : IAccountService
{
    // Role -> Employee ID prefix mapping used for auto-generated IDs.
    private static readonly Dictionary<string, string> RolePrefixes = new()
    {
        { UserRoles.Employee, "EMP" },
        { UserRoles.Admin, "ADM" },
        { UserRoles.SupportStaff, "STF" }
    };

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
    // REGISTER (Admin only — called from an [Authorize(Roles = Admin)] action)
    // =========================================================

    public async Task<ServiceResult> RegisterAsync(
        RegisterViewModel model)
    {
        // Normalize input
        var email = model.Email.Trim().ToLowerInvariant();
        var name = model.Name.Trim();
        var mobile = model.Mobile.Trim();
        var role = model.Role.Trim();

        // -----------------------------------------------------
        // 1. Check the requested role is valid
        // -----------------------------------------------------

        if (!RolePrefixes.TryGetValue(role, out var prefix))
        {
            return ServiceResult.Failure(
                "Please select a valid role.");
        }

        // -----------------------------------------------------
        // 2. Check duplicate email
        // -----------------------------------------------------

        var existingEmail =
            await _userManager.FindByEmailAsync(email);

        if (existingEmail != null)
        {
            return ServiceResult.Failure(
                "An account with this email already exists.");
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
        // 4. Auto-generate the Employee ID based on role
        // -----------------------------------------------------

        var employeeId =
            await GenerateEmployeeIdAsync(prefix);

        // -----------------------------------------------------
        // 5. Create Identity User
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
        // 6. Create user using ASP.NET Core Identity
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
        // 7. Make sure the selected role exists
        // -----------------------------------------------------

        if (!await _roleManager.RoleExistsAsync(role))
        {
            var roleResult =
                await _roleManager.CreateAsync(
                    new IdentityRole<int>(role));

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
        // 8. Assign the selected role
        // -----------------------------------------------------

        var roleAssignmentResult =
            await _userManager.AddToRoleAsync(
                user,
                role);

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
        // 9. Registration successful
        // -----------------------------------------------------

        return ServiceResult.Success();
    }


    // =========================================================
    // EMPLOYEE ID GENERATION
    // =========================================================

    // Generates the next sequential ID for a given role prefix,
    // e.g. "EMP0004", "ADM0002", "STF0007".
    private async Task<string> GenerateEmployeeIdAsync(string prefix)
    {
        var nextSequence =
            await _userRepository.GetNextSequenceAsync(prefix);

        var candidateId = $"{prefix}{nextSequence:D4}";

        // Safety net in case of a race condition between the
        // sequence lookup and the insert (two admins registering
        // at the same moment). Keep incrementing until free.
        while (await _userRepository.EmployeeIdExistsAsync(candidateId))
        {
            nextSequence++;
            candidateId = $"{prefix}{nextSequence:D4}";
        }

        return candidateId;
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

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        return await _departmentRepository.GetAllAsync();
    }
}