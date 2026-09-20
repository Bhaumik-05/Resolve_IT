using Microsoft.EntityFrameworkCore;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Data;


namespace rsit.Repositories
{
    public class UserRepository:IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetByEmployeeIdAsync(string employeeId)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.EmployeeId == employeeId);
        }

        public async Task<bool> EmployeeIdExistsAsync(string employeeId)
        {
            return await _context.Users
                .AnyAsync(u => u.EmployeeId == employeeId);
        }

        public async Task<int> GetNextSequenceAsync(string prefix)
        {
            var ids = await _context.Users
                .Where(u => u.EmployeeId.StartsWith(prefix))
                .Select(u => u.EmployeeId)
                .ToListAsync();

            int maxSeq = 0;
            foreach (var id in ids)
            {
                var numericPart = id[prefix.Length..];
                if (int.TryParse(numericPart, out var seq) && seq > maxSeq)
                    maxSeq = seq;
            }

            return maxSeq + 1;
        }

        public async Task<(List<User> Items, int Total)> SearchAsync(
            string? search, int? departmentId, string? status, string? role,
            int page, int pageSize)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(u => u.Department)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(u =>
                    u.Name.ToLower().Contains(term) ||
                    (u.Email != null && u.Email.ToLower().Contains(term)) ||
                    u.EmployeeId.ToLower().Contains(term));
            }

            if (departmentId.HasValue)
                query = query.Where(u => u.DepartmentId == departmentId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(u => u.AccountStatus == status);

            if (!string.IsNullOrWhiteSpace(role))
            {
                query = query.Where(u =>
                    _context.UserRoles.Any(ur =>
                        ur.UserId == u.Id &&
                        _context.Roles.Any(r => r.Id == ur.RoleId && r.Name == role)));
            }

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(u => u.Name)
                .ThenBy(u => u.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, total);
        }

        public async Task<Dictionary<int, string>> GetRolesByUserIdsAsync(IEnumerable<int> userIds)
        {
            var ids = userIds.ToList();

            var pairs = await (
                from ur in _context.UserRoles
                join r in _context.Roles on ur.RoleId equals r.Id
                where ids.Contains(ur.UserId)
                select new { ur.UserId, RoleName = r.Name })
                .ToListAsync();

            return pairs
                .GroupBy(p => p.UserId)
                .ToDictionary(g => g.Key, g => g.First().RoleName ?? string.Empty);
        }
    }
}
