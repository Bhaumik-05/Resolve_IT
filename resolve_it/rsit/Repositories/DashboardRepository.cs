using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Repositories.Projections;

namespace rsit.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private const int TopN = 6;
        private const int RecentCount = 8;

        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardData> GetDashboardDataAsync(int trendDays)
        {
            var finished = TicketStatuses.Finished;
            var tickets = _context.Tickets.AsNoTracking();
            var data = new DashboardData();

            // ---------- Ticket headline numbers ----------
            data.TotalTickets = await tickets.CountAsync();
            data.OpenTickets = await tickets.CountAsync(t => !finished.Contains(t.Status));
            data.FinishedTickets = data.TotalTickets - data.OpenTickets;
            data.UnassignedTickets = await tickets.CountAsync(t =>
                !finished.Contains(t.Status) && !t.Assignments.Any());

            var weekAgo = DateTime.UtcNow.AddDays(-7);
            data.CreatedLast7Days = await tickets.CountAsync(t => t.CreatedAt >= weekAgo);

            // ---------- Distributions ----------
            data.ByStatus = await tickets
                .GroupBy(t => t.Status)
                .Select(g => new LabelCount { Label = g.Key, Count = g.Count() })
                .ToListAsync();

            data.ByPriority = await tickets
                .GroupBy(t => t.Priority)
                .Select(g => new LabelCount { Label = g.Key, Count = g.Count() })
                .ToListAsync();

            data.ByDepartment = await tickets
                .GroupBy(t => t.Department.Name)
                .Select(g => new LabelCount { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(TopN)
                .ToListAsync();

            data.ByCategory = await tickets
                .GroupBy(t => t.Category.Name)
                .Select(g => new LabelCount { Label = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(TopN)
                .ToListAsync();

            // ---------- Daily trend (grouped client-side; provider independent) ----------
            var firstDay = DateTime.UtcNow.Date.AddDays(-(trendDays - 1));
            var created = await tickets
                .Where(t => t.CreatedAt >= firstDay)
                .Select(t => t.CreatedAt)
                .ToListAsync();

            var perDay = created
                .GroupBy(d => d.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            for (var i = 0; i < trendDays; i++)
            {
                var day = firstDay.AddDays(i);
                data.Trend.Add(new DailyCount
                {
                    Date = day,
                    Count = perDay.TryGetValue(day, out var c) ? c : 0
                });
            }

            // ---------- Average resolution time ----------
            var resolved = await tickets
                .Where(t => t.ResolvedAt != null)
                .Select(t => new { t.CreatedAt, t.ResolvedAt })
                .ToListAsync();

            if (resolved.Count > 0)
            {
                data.AverageResolutionHours = resolved
                    .Average(t => (t.ResolvedAt!.Value - t.CreatedAt).TotalHours);
            }

            // ---------- Feedback ----------
            data.AverageRating = await _context.Feedbacks
                .Select(f => (double?)f.Rating)
                .AverageAsync();

            // ---------- Recent tickets ----------
            data.RecentTickets = await tickets
                .OrderByDescending(t => t.CreatedAt)
                .Take(RecentCount)
                .Select(t => new RecentTicketRow
                {
                    TicketId = t.TicketId,
                    Title = t.Title,
                    Priority = t.Priority,
                    Status = t.Status,
                    EmployeeName = t.Employee.Name,
                    DepartmentName = t.Department.Name,
                    CreatedAt = t.CreatedAt
                })
                .ToListAsync();

            // ---------- Users ----------
            data.TotalUsers = await _context.Users.CountAsync();
            data.ActiveUsers = await _context.Users
                .CountAsync(u => u.AccountStatus == RecordStatus.Active);

            data.UsersByRole = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                select r.Name)
                .GroupBy(name => name)
                .Select(g => new LabelCount { Label = g.Key ?? "", Count = g.Count() })
                .ToListAsync();

            // ---------- Departments & categories ----------
            data.TotalDepartments = await _context.Departments.CountAsync();
            data.ActiveDepartments = await _context.Departments
                .CountAsync(d => d.Status == RecordStatus.Active);

            data.TotalCategories = await _context.Categories.CountAsync();
            data.ActiveCategories = await _context.Categories
                .CountAsync(c => c.Status == RecordStatus.Active);

            return data;
        }
    }
}
