using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.ViewModels;

namespace rsit.Controllers;

[Authorize]
public class TicketsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public TicketsController(
        ApplicationDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Tickets/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateTicketViewModel
        {
            Priorities = TicketPriorities.All,

            Categories = await _context.Categories
                .Where(c => c.Status == RecordStatus.Active)
                .OrderBy(c => c.Name)
                .ToListAsync(),

            Departments = await _context.Departments
                .Where(d => d.Status == RecordStatus.Active)
                .OrderBy(d => d.Name)
                .ToListAsync()
        };

        return View(model);
    }

    // POST: /Tickets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Priorities = TicketPriorities.All;

            model.Categories = await _context.Categories
                .Where(c => c.Status == RecordStatus.Active)
                .OrderBy(c => c.Name)
                .ToListAsync();

            model.Departments = await _context.Departments
                .Where(d => d.Status == RecordStatus.Active)
                .OrderBy(d => d.Name)
                .ToListAsync();

            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var ticket = new Ticket
        {
            Title = model.Title.Trim(),
            Description = model.Description.Trim(),
            Priority = model.Priority,
            Status = TicketStatuses.New,
            CreatedAt = DateTime.UtcNow,

            EmployeeId = user.Id,
            CategoryId = model.CategoryId,
            DepartmentId = model.DepartmentId
        };

        _context.Tickets.Add(ticket);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            $"Ticket #{ticket.TicketId} created successfully.";

        return RedirectToAction(nameof(Details), new { id = ticket.TicketId });
    }

    // GET: /Tickets/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var ticket = await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.Employee)
            .Include(t => t.Attachments)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.Staff)
            .Include(t => t.History)
            .Include(t => t.Feedback)
            .FirstOrDefaultAsync(t => t.TicketId == id);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }
}