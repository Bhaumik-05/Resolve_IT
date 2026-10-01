
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
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


    // =========================================================
    // LOGIN
    // =========================================================

    // GET: /Account/Login
    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction(nameof(RouteToDashboard));
        }

        return View(new LoginViewModel());
    }


    // POST: /Account/Login
    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _accountService.LoginAsync(model);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }

            return View(model);
        }

        return RedirectToAction(nameof(RouteToDashboard));
    }


    // =========================================================
    // ROLE-BASED DASHBOARD ROUTING
    // =========================================================

    [Authorize]
    [HttpGet]
    public IActionResult RouteToDashboard()
    {
        if (User.IsInRole(UserRoles.Admin))
        {
            return RedirectToAction("Index", "Admin");
        }

        if (User.IsInRole(UserRoles.SupportStaff))
        {
            return RedirectToAction("Index", "Support");
        }

        if (User.IsInRole(UserRoles.Employee))
        {
            return RedirectToAction("Index", "Employee");
        }

        return RedirectToAction(nameof(AccessDenied));
    }


    // =========================================================
    // ACCESS DENIED
    // =========================================================

    [AllowAnonymous]
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }


    // =========================================================
    // LOGOUT
    // =========================================================

    // POST: /Account/Logout
    [Authorize]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _accountService.LogoutAsync();

        return RedirectToAction(nameof(Login));
    }
}