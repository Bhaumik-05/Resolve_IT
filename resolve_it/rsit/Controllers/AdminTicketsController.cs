using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Route("Admin/Tickets")]
public class AdminTicketsController : Controller
{
    private readonly IAdminTicketService _ticketService;

    public AdminTicketsController(
        IAdminTicketService ticketService)
    {
        _ticketService = ticketService;
    }

    private int CurrentUserId =>
        int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // GET: /Admin/Tickets
    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] AdminTicketListViewModel query)
    {
        var model = await _ticketService.GetListAsync(query);

        return View(model);
    }

    // GET: /Admin/Tickets/Details/5
    [HttpGet("Details/{id:int}")]
    public async Task<IActionResult> Details(int id)
    {
        var model = await _ticketService.GetDetailsAsync(id);

        if (model == null)
            return NotFound();

        return View(model);
    }

    // POST: /Admin/Tickets/Assign/5
    [HttpPost("Assign/{id:int}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Assign(
        int id,
        AssignTicketViewModel model)
    {
        model.TicketId = id;

        var result = await _ticketService.AssignTicketAsync(
            model,
            CurrentUserId);

        if (result.Succeeded)
        {
            TempData["SuccessMessage"] =
                "Ticket assigned successfully.";
        }
        else
        {
            TempData["ErrorMessage"] =
                string.Join(" ", result.Errors);
        }

        return RedirectToAction(
            nameof(Details),
            new { id });
    }
}