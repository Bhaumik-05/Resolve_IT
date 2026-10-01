using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories.Interfaces;

namespace rsit.Repositories;

public class AdminFeedbackRepository : IAdminFeedbackRepository
{
    private readonly ApplicationDbContext _context;

    public AdminFeedbackRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Feedback>> SearchAsync(
        string? search,
        int? rating)
    {
        var query = _context.Feedbacks
            .AsNoTracking()
            .Include(f => f.Employee)
            .Include(f => f.Ticket)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(f =>
                f.Comments.ToLower().Contains(term) ||
                f.Employee.Name.ToLower().Contains(term) ||
                f.Employee.EmployeeId.ToLower().Contains(term) ||
                f.Ticket.Title.ToLower().Contains(term));
        }

        if (rating.HasValue)
        {
            query = query.Where(f => f.Rating == rating.Value);
        }

        return await query
            .OrderByDescending(f => f.SubmittedAt)
            .ToListAsync();
    }
}