using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
using rsit.Services;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Controllers;

/// <summary>FR-18: manage departments.</summary>
[Authorize(Roles = UserRoles.Admin)]
[Route("Admin/Departments")]
public class DepartmentsController : Controller
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] DepartmentListViewModel query)
    {
        return View(await _departmentService.GetListAsync(query));
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new DepartmentFormViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DepartmentFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _departmentService.CreateAsync(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Department '{model.Name.Trim()}' was created.";
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
        }

        return View(model);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _departmentService.GetForEditAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DepartmentFormViewModel model)
    {
        model.Id = id;

        if (ModelState.IsValid)
        {
            var result = await _departmentService.UpdateAsync(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Department '{model.Name.Trim()}' was updated.";
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
        }

        return View(model);
    }

    [HttpPost("SetStatus/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, bool activate)
    {
        var result = await _departmentService.SetStatusAsync(id, activate);

        if (result.Succeeded)
            TempData["SuccessMessage"] = activate ? "Department activated." : "Department deactivated.";
        else
            TempData["ErrorMessage"] = string.Join(" ", result.Errors);

        return RedirectToAction(nameof(Index));
    }

    private void AddErrors(ServiceResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error);
    }
}
