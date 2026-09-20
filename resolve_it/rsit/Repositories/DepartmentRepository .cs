using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Repositories.Projections;

namespace rsit.Repositories
{
    public class DepartmentRepository:IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int departmentId)
        {
            return await _context.Departments
                .AnyAsync(d => d.DepartmentId == departmentId);
        }

        public async Task<List<Department>> GetAllAsync()
        {
            return await _context.Departments
                .Where(d => d.Status == RecordStatus.Active)
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<List<Department>> GetActiveOrCurrentAsync(int? currentDepartmentId)
        {
            return await _context.Departments
                .AsNoTracking()
                .Where(d => d.Status == RecordStatus.Active ||
                            (currentDepartmentId != null && d.DepartmentId == currentDepartmentId))
                .OrderBy(d => d.Name)
                .ToListAsync();
        }

        public async Task<Department?> GetByIdAsync(int departmentId)
        {
            return await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeDepartmentId)
        {
            var normalized = name.Trim().ToLower();

            return await _context.Departments.AnyAsync(d =>
                d.Name.ToLower() == normalized &&
                (excludeDepartmentId == null || d.DepartmentId != excludeDepartmentId));
        }

        public async Task<List<DepartmentSummary>> GetSummariesAsync(string? search, string? status)
        {
            var query = _context.Departments.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(d =>
                    d.Name.ToLower().Contains(term) ||
                    d.Description.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(d => d.Status == status);

            var summaries = await query
                .OrderBy(d => d.Name)
                .Select(d => new DepartmentSummary
                {
                    DepartmentId = d.DepartmentId,
                    Name = d.Name,
                    Description = d.Description,
                    Status = d.Status,
                    TicketCount = d.Tickets.Count()
                })
                .ToListAsync();

            // Employees / support staff per department, grouped in SQL.
            var roleCounts = await (
                from u in _context.Users
                join ur in _context.UserRoles on u.Id equals ur.UserId
                join r in _context.Roles on ur.RoleId equals r.Id
                select new { u.DepartmentId, RoleName = r.Name })
                .GroupBy(x => new { x.DepartmentId, x.RoleName })
                .Select(g => new { g.Key.DepartmentId, g.Key.RoleName, Count = g.Count() })
                .ToListAsync();

            foreach (var summary in summaries)
            {
                summary.EmployeeCount = roleCounts
                    .Where(c => c.DepartmentId == summary.DepartmentId && c.RoleName == UserRoles.Employee)
                    .Sum(c => c.Count);

                summary.SupportStaffCount = roleCounts
                    .Where(c => c.DepartmentId == summary.DepartmentId && c.RoleName == UserRoles.SupportStaff)
                    .Sum(c => c.Count);
            }

            return summaries;
        }

        public async Task AddAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }
    }
}
