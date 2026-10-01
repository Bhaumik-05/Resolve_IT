using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
using rsit.Services.Interfaces;

namespace rsit.Controllers;

/// <summary>FR-20: admin dashboard.</summary>
[Authorize(Roles = UserRoles.Admin)]
public class AdminController : Controller
{
    private readonly IDashboardService _dashboardService;

    public AdminController(
        IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(
        DateTime? fromDate,
        DateTime? toDate)
    {
        var model =
            await _dashboardService
                .GetAdminDashboardAsync(
                    fromDate,
                    toDate);

        return View(model);
    }
}