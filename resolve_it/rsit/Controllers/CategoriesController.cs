using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
using rsit.Services;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Controllers;

/// <summary>FR-19: manage complaint categories.</summary>
[Authorize(Roles = UserRoles.Admin)]
[Route("Admin/Categories")]
public class CategoriesController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index([FromQuery] CategoryListViewModel query)
    {
        return View(await _categoryService.GetListAsync(query));
    }

    [HttpGet("Create")]
    public IActionResult Create()
    {
        return View(new CategoryFormViewModel());
    }

    [HttpPost("Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (ModelState.IsValid)
        {
            var result = await _categoryService.CreateAsync(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Category '{model.Name.Trim()}' was created.";
                return RedirectToAction(nameof(Index));
            }

            AddErrors(result);
        }

        return View(model);
    }

    [HttpGet("Edit/{id:int}")]
    public async Task<IActionResult> Edit(int id)
    {
        var model = await _categoryService.GetForEditAsync(id);
        return model == null ? NotFound() : View(model);
    }

    [HttpPost("Edit/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, CategoryFormViewModel model)
    {
        model.Id = id;

        if (ModelState.IsValid)
        {
            var result = await _categoryService.UpdateAsync(model);

            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Category '{model.Name.Trim()}' was updated.";
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
        var result = await _categoryService.SetStatusAsync(id, activate);

        if (result.Succeeded)
            TempData["SuccessMessage"] = activate ? "Category activated." : "Category deactivated.";
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
