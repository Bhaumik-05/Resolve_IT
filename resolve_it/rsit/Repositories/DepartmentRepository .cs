using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories.Interfaces;

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
                .Where(d => d.Status == "Active")
                .ToListAsync();
        }
    }
}
