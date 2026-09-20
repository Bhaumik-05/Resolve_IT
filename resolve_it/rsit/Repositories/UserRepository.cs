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
    }
}
