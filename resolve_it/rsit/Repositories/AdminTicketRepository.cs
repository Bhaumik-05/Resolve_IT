using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories.Interfaces;

namespace rsit.Repositories;

public class AdminTicketRepository : IAdminTicketRepository
{
    private readonly ApplicationDbContext _context;

    public AdminTicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Ticket>> SearchAsync(
        string? search,
        string? status,
        string? priority,
        int? departmentId)
    {
        var query = _context.Tickets
            .AsNoTracking()
            .Include(t => t.Employee)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.Staff)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(t =>
                t.Title.ToLower().Contains(term) ||
                t.Description.ToLower().Contains(term) ||
                t.Employee.Name.ToLower().Contains(term) ||
                t.Employee.EmployeeId.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (!string.IsNullOrWhiteSpace(priority))
            query = query.Where(t => t.Priority == priority);

        if (departmentId.HasValue)
            query = query.Where(t => t.DepartmentId == departmentId.Value);

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Ticket?> GetDetailsAsync(int ticketId)
    {
        return await _context.Tickets
            .AsNoTracking()
            .Include(t => t.Employee)
            .Include(t => t.Department)
            .Include(t => t.Category)
            .Include(t => t.Attachments)
                .ThenInclude(a => a.Uploader)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.Staff)
            .Include(t => t.Assignments)
                .ThenInclude(a => a.AssignedByUser)
            .Include(t => t.History)
                .ThenInclude(h => h.ChangedByUser)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    }

    public async Task<List<User>> GetActiveSupportStaffAsync()
    {
        var staff = await (
            from user in _context.Users
            join userRole in _context.UserRoles
                on user.Id equals userRole.UserId
            join role in _context.Roles
                on userRole.RoleId equals role.Id
            where role.Name == UserRoles.SupportStaff
                  && user.AccountStatus == RecordStatus.Active
            orderby user.Name
            select user
        )
        .AsNoTracking()
        .ToListAsync();

        return staff;
    }

    public async Task<Assignment?> GetCurrentAssignmentAsync(int ticketId)
    {
        return await _context.Assignments
            .Include(a => a.Staff)
            .Include(a => a.AssignedByUser)
            .Where(a => a.TicketId == ticketId)
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAssignmentAsync(Assignment assignment)
    {
        await _context.Assignments.AddAsync(assignment);
    }

    public async Task AddHistoryAsync(TicketHistory history)
    {
        await _context.TicketHistories.AddAsync(history);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}