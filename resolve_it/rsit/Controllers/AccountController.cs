using Microsoft.AspNetCore.Mvc;
using rsit.Services.Interfaces;
using rsit.ViewModels;

namespace rsit.Controllers;

public class AccountController : Controller
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }


    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }


    // POST: /Account/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result =
            await _accountService.RegisterAsync(model);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error);
            }

            return View(model);
        }

        TempData["SuccessMessage"] =
            "Account created successfully. Please login.";

        return RedirectToAction(nameof(Login));
    }


    // GET: /Account/Login
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }


    // POST: /Account/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        LoginViewModel model,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result =
            await _accountService.LoginAsync(model);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error);
            }

            return View(model);
        }

        // Prevent open redirect attacks.
        if (!string.IsNullOrWhiteSpace(returnUrl) &&
            Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        // Temporary dashboard routing.
        // We'll replace these with actual dashboards later.
        if (User.IsInRole("Admin"))
        {
            return RedirectToAction(
                "Index",
                "Admin");
        }

        if (User.IsInRole("Support Staff"))
        {
            return RedirectToAction(
                "Index",
                "Support");
        }

        return RedirectToAction(
            "Index",
            "Employee");
    }


    // POST: /Account/Logout
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();

        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}