using Microsoft.AspNetCore.Identity;
using rsit.Models;
using rsit.Services.Interfaces;
using rsit.ViewModels;

namespace rsit.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountService(
        UserManager<User> userManager,
        SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
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