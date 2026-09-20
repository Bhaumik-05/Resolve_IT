using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
using rsit.Services;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Controllers;

/// <summary>FR-17: manage users.</summary>
[Authorize(Roles = UserRoles.Admin)]
[Route("Admin/Users")]
public class UsersController : Controller
{
    private readonly IUserManagementService _userService;

    public UsersController(IUserManagementService userService)
    {
        _userService = userService;
    }

    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // ---------------- LIST ----------------

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] UserListViewModel query)
    {
        var model = await _userService.GetUserListAsync(query, CurrentUserId);
        return View(model);
    }

    // ---------------- CREATE ----------------

    [HttpGet("Create")]
    public async Task<IActionResult> Create()
    {
        var model = new CreateUserViewModel();
        await _userService.PopulateOptionsAsync(model);
        return View(model);
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _userService.CreateUserAsync(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{model.Name.Trim()}' was created.";
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
        }

        await _userService.PopulateOptionsAsync(model);
        return View(model);
    }

    // ---------------- EDIT ----------------

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _userService.GetUserForEditAsync(id, CurrentUserId);

        if (model == null)
            return NotFound();

        await _userService.PopulateOptionsAsync(model);
        return View(model);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, UserFormViewModel model)
    {
        model.Id = id;

        if (ModelState.IsValid)
        {
            var result = await _userService.UpdateUserAsync(model, CurrentUserId);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User '{model.Name.Trim()}' was updated.";
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
        }

        // Re-populate the read-only parts that are not posted back.
        var existing = await _userService.GetUserForEditAsync(id, CurrentUserId);

        if (existing == null)
            return NotFound();

        model.EmployeeId = existing.EmployeeId;
        model.IsSelf = existing.IsSelf;

        await _userService.PopulateOptionsAsync(model);
        return View(model);
    }

    // ---------------- ACTIVATE / DEACTIVATE ----------------

    [HttpPost("SetStatus/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, bool activate)
    {
        var result = await _userService.SetUserStatusAsync(id, activate, CurrentUserId);

        if (result.Succeeded)
            TempData["SuccessMessage"] = activate ? "User activated." : "User deactivated.";
        else
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Index));
    }

    // ---------------- RESET PASSWORD ----------------

    [HttpPost("ResetPassword/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(int id, ResetPasswordViewModel model)
    {
        model.Id = id;

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = string.Join(" ",
                ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));

            return RedirectToAction(nameof(Edit), new { id });
        }

        var result = await _userService.ResetPasswordAsync(model);

        if (result.Succeeded)
            TempData["SuccessMessage"] = "Password was reset.";
        else
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Edit), new { id });
    }

    private void AddErrors(ServiceResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error);
    }
}
