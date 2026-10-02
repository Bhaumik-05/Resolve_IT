using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Services.Interfaces;
using rsit.ViewModels.Support;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.SupportStaff)]
public class SupportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;
    private readonly ITicketService _ticketService;

    public SupportController(
        ApplicationDbContext context,
        UserManager<User> userManager,
        ITicketService ticketService)
    {
        _context = context;
        _userManager = userManager;
        _ticketService = ticketService;
    }


    // =========================================================
    // SUPPORT STAFF DASHBOARD
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var staffId))
        {
            return Challenge();
        }

        var staff = await _userManager.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id == staffId);

        if (staff == null)
        {
            return Challenge();
        }

        var assignments = await _context.Assignments
            .Where(a => a.StaffId == staffId)
            .Include(a => a.Ticket)
                .ThenInclude(t => t.Employee)
            .Include(a => a.Ticket)
                .ThenInclude(t => t.Category)
            .OrderByDescending(a => a.AssignedAt)
            .ToListAsync();

        var tickets = assignments
            .Select(a => a.Ticket)
            .DistinctBy(t => t.TicketId)
            .ToList();

        var model = new SupportDashboardViewModel
        {
            StaffName = staff.Name,
            StaffEmployeeId = staff.EmployeeId,
            DepartmentName = staff.Department?.Name ?? "N/A",

            AssignedTickets = tickets.Count,

            NewTickets = tickets.Count(t =>
                t.Status == TicketStatuses.New ||
                t.Status == TicketStatuses.Assigned),

            InProgressTickets = tickets.Count(t =>
                t.Status == TicketStatuses.InProgress),

            ResolvedTickets = tickets.Count(t =>
                t.Status == TicketStatuses.Resolved),

            CriticalTickets = tickets.Count(t =>
                t.Priority == "Critical" &&
                t.Status != TicketStatuses.Resolved &&
                t.Status != TicketStatuses.Closed),

            RecentTickets = tickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(8)
                .Select(t => new SupportTicketRowViewModel
                {
                    TicketId = t.TicketId,
                    Title = t.Title,
                    EmployeeName = t.Employee?.Name ?? "N/A",
                    CategoryName = t.Category?.Name ?? "N/A",
                    Priority = t.Priority,
                    Status = t.Status,
                    CreatedAt = t.CreatedAt
                })
                .ToList()
        };

        return View(model);
    }


    // =========================================================
    // TICKET DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var staffId))
        {
            return Challenge();
        }

        // Get the ticket with all required details
        var ticket = await _context.Tickets
            .Include(t => t.Employee)
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.Attachments)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.Staff)
            .Include(t => t.History)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        // Staff can only view tickets assigned to them
        var isAssigned = ticket.Assignments
            .Any(a => a.StaffId == staffId);

        if (!isAssigned)
        {
            return Forbid();
        }

        return View(ticket);
    }


    // =========================================================
    // UPDATE TICKET STATUS
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(
        SupportStatusUpdateViewModel model)
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var staffId))
        {
            return Challenge();
        }

        // -----------------------------------------------------
        // Validate requested status
        // -----------------------------------------------------

        if (model.Status != TicketStatuses.InProgress)
        {
            TempData["ErrorMessage"] =
                "Invalid status update.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.TicketId });
        }


        // -----------------------------------------------------
        // Update status through TicketService
        // -----------------------------------------------------

        var success =
            await _ticketService.UpdateTicketStatusAsync(
                model.TicketId,
                staffId,
                model.Status);


        // -----------------------------------------------------
        // Handle failed update
        // -----------------------------------------------------

        if (!success)
        {
            TempData["ErrorMessage"] =
                "Unable to update the ticket status. " +
                "Make sure the ticket is assigned to you " +
                "and can be moved to In Progress.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.TicketId });
        }


        // -----------------------------------------------------
        // Success
        // -----------------------------------------------------

        TempData["SuccessMessage"] =
            "Ticket status updated to In Progress.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.TicketId });
    }


    // =========================================================
    // RESOLVE TICKET
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResolveTicket(
        SupportResolveViewModel model)
    {
        var userId = _userManager.GetUserId(User);

        if (!int.TryParse(userId, out var staffId))
        {
            return Challenge();
        }

        // -----------------------------------------------------
        // Validate resolution remarks
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(model.Remarks))
        {
            TempData["ErrorMessage"] =
                "Resolution remarks are required before resolving the ticket.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.TicketId });
        }


        // -----------------------------------------------------
        // Resolve ticket through TicketService
        // -----------------------------------------------------

        var success =
            await _ticketService.ResolveTicketAsync(
                model.TicketId,
                staffId,
                model.Remarks);


        // -----------------------------------------------------
        // Handle failed resolution
        // -----------------------------------------------------

        if (!success)
        {
            TempData["ErrorMessage"] =
                "Unable to resolve the ticket. " +
                "Make sure the ticket is assigned to you " +
                "and is currently In Progress.";

            return RedirectToAction(
                nameof(Details),
                new { id = model.TicketId });
        }


        // -----------------------------------------------------
        // Success
        // -----------------------------------------------------

        TempData["SuccessMessage"] =
            "Ticket has been resolved successfully.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.TicketId });
    }
}