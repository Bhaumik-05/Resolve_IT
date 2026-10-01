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

        public async Task<DashboardData> GetDashboardDataAsync(
            int trendDays,
            DateTime? fromDate = null,
            DateTime? toDate = null)
        {
            var finished = TicketStatuses.Finished;

            var tickets = _context.Tickets
                .AsNoTracking()
                .AsQueryable();

            /*
             * Date filtering is based on Ticket.CreatedAt.
             *
             * From date is inclusive.
             * To date is inclusive until the end of that day.
             */

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;

                tickets = tickets.Where(t =>
                    t.CreatedAt >= from);
            }

            if (toDate.HasValue)
            {
                var toExclusive = toDate.Value.Date.AddDays(1);

                tickets = tickets.Where(t =>
                    t.CreatedAt < toExclusive);
            }

            var data = new DashboardData();


            // =========================================================
            // TICKET HEADLINE NUMBERS
            // =========================================================

            data.TotalTickets =
                await tickets.CountAsync();

            data.OpenTickets =
                await tickets.CountAsync(t =>
                    !finished.Contains(t.Status));

            data.FinishedTickets =
                data.TotalTickets - data.OpenTickets;

            data.UnassignedTickets =
                await tickets.CountAsync(t =>
                    !finished.Contains(t.Status) &&
                    !t.Assignments.Any());


            // =========================================================
            // CREATED TICKETS IN SELECTED RANGE
            // =========================================================

            if (fromDate.HasValue || toDate.HasValue)
            {
                data.CreatedLast7Days =
                    data.TotalTickets;
            }
            else
            {
                var weekAgo =
                    DateTime.UtcNow.AddDays(-7);

                data.CreatedLast7Days =
                    await _context.Tickets
                        .AsNoTracking()
                        .CountAsync(t =>
                            t.CreatedAt >= weekAgo);
            }


            // =========================================================
            // STATUS
            // =========================================================

            data.ByStatus = await tickets
                .GroupBy(t => t.Status)
                .Select(g => new LabelCount
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();


            // =========================================================
            // PRIORITY
            // =========================================================

            data.ByPriority = await tickets
                .GroupBy(t => t.Priority)
                .Select(g => new LabelCount
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();


            // =========================================================
            // DEPARTMENT
            // =========================================================

            data.ByDepartment = await tickets
                .GroupBy(t => t.Department.Name)
                .Select(g => new LabelCount
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(TopN)
                .ToListAsync();


            // =========================================================
            // CATEGORY
            // =========================================================

            data.ByCategory = await tickets
                .GroupBy(t => t.Category.Name)
                .Select(g => new LabelCount
                {
                    Label = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(TopN)
                .ToListAsync();


            // =========================================================
            // DAILY TREND
            // =========================================================

            DateTime firstDay;
            DateTime lastDay;

            if (fromDate.HasValue || toDate.HasValue)
            {
                firstDay = fromDate?.Date
                    ?? (toDate!.Value.Date.AddDays(-(trendDays - 1)));

                lastDay = toDate?.Date
                    ?? DateTime.UtcNow.Date;

                if (lastDay < firstDay)
                {
                    (firstDay, lastDay) =
                        (lastDay, firstDay);
                }
            }
            else
            {
                lastDay = DateTime.UtcNow.Date;

                firstDay =
                    lastDay.AddDays(-(trendDays - 1));
            }

            var trendTickets = await tickets
                .Where(t =>
                    t.CreatedAt >= firstDay &&
                    t.CreatedAt < lastDay.AddDays(1))
                .Select(t => t.CreatedAt)
                .ToListAsync();

            var perDay = trendTickets
                .GroupBy(d => d.Date)
                .ToDictionary(
                    g => g.Key,
                    g => g.Count());


            for (var day = firstDay;
                 day <= lastDay;
                 day = day.AddDays(1))
            {
                data.Trend.Add(new DailyCount
                {
                    Date = day,
                    Count = perDay.TryGetValue(
                        day,
                        out var count)
                        ? count
                        : 0
                });
            }


            // =========================================================
            // AVERAGE RESOLUTION TIME
            // =========================================================

            var resolved = await tickets
                .Where(t => t.ResolvedAt != null)
                .Select(t => new
                {
                    t.CreatedAt,
                    t.ResolvedAt
                })
                .ToListAsync();

            if (resolved.Count > 0)
            {
                data.AverageResolutionHours =
                    resolved.Average(t =>
                        (t.ResolvedAt!.Value -
                         t.CreatedAt).TotalHours);
            }


            // =========================================================
            // FEEDBACK
            // =========================================================

            var feedbackQuery =
                _context.Feedbacks
                    .AsNoTracking()
                    .AsQueryable();

            if (fromDate.HasValue)
            {
                var from = fromDate.Value.Date;

                feedbackQuery =
                    feedbackQuery.Where(f =>
                        f.SubmittedAt >= from);
            }

            if (toDate.HasValue)
            {
                var toExclusive =
                    toDate.Value.Date.AddDays(1);

                feedbackQuery =
                    feedbackQuery.Where(f =>
                        f.SubmittedAt < toExclusive);
            }

            data.AverageRating =
                await feedbackQuery
                    .Select(f => (double?)f.Rating)
                    .AverageAsync();


            // =========================================================
            // RECENT TICKETS
            // =========================================================

            data.RecentTickets =
                await tickets
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


            // =========================================================
            // USERS
            // These remain system-wide metrics.
            // =========================================================

            data.TotalUsers =
                await _context.Users.CountAsync();

            data.ActiveUsers =
                await _context.Users
                    .CountAsync(u =>
                        u.AccountStatus ==
                        RecordStatus.Active);

            data.UsersByRole =
                await (
                    from ur in _context.UserRoles
                    join r in _context.Roles
                        on ur.RoleId equals r.Id
                    select r.Name)
                .GroupBy(name => name)
                .Select(g => new LabelCount
                {
                    Label = g.Key ?? "",
                    Count = g.Count()
                })
                .ToListAsync();


            // =========================================================
            // DEPARTMENTS & CATEGORIES
            // System-wide metrics.
            // =========================================================

            data.TotalDepartments =
                await _context.Departments.CountAsync();

            data.ActiveDepartments =
                await _context.Departments
                    .CountAsync(d =>
                        d.Status ==
                        RecordStatus.Active);

            data.TotalCategories =
                await _context.Categories.CountAsync();

            data.ActiveCategories =
                await _context.Categories
                    .CountAsync(c =>
                        c.Status ==
                        RecordStatus.Active);

            return data;
        }
    }
}