using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.ViewModels.Support;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.SupportStaff)]
public class SupportController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<User> _userManager;

    public SupportController(
        ApplicationDbContext context,
        UserManager<User> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

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
}