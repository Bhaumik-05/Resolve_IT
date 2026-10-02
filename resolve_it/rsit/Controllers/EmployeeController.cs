using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rsit.Models;
using rsit.Services.Interfaces;
using rsit.ViewModels.Employee;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.Employee)]
public class EmployeeController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly ITicketService _ticketService;

    public EmployeeController(
        UserManager<User> userManager,
        ITicketService ticketService)
    {
        _userManager = userManager;
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
        {
            return Challenge();
        }

        var user = await _userManager.Users
            .Include(u => u.Department)
            .FirstOrDefaultAsync(u => u.Id.ToString() == userId);

        if (user == null)
        {
            return Challenge();
        }

        var tickets = await _ticketService
            .GetTicketsForEmployeeAsync(user.Id);

        var model = new EmployeeDashboardViewModel
        {
            EmployeeName = user.Name,
            EmployeeId = user.EmployeeId,
            DepartmentName = user.Department?.Name ?? "N/A",

            TotalTickets = tickets.Count,

            NewTickets = tickets.Count(t =>
                t.Status == TicketStatuses.New),

            AssignedTickets = tickets.Count(t =>
                t.Status == TicketStatuses.Assigned),

            InProgressTickets = tickets.Count(t =>
                t.Status == TicketStatuses.InProgress),

            ResolvedTickets = tickets.Count(t =>
                t.Status == TicketStatuses.Resolved),

            ClosedTickets = tickets.Count(t =>
                t.Status == TicketStatuses.Closed),

            RecentTickets = tickets
                .Take(5)
                .Select(t => new EmployeeTicketRowViewModel
                {
                    TicketId = t.TicketId,
                    Title = t.Title,
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