using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Repositories.Projections;

namespace rsit.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Category>> GetActiveAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .Where(c => c.Status == RecordStatus.Active)
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int categoryId)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);
        }

        public async Task<bool> NameExistsAsync(string name, int? excludeCategoryId)
        {
            var normalized = name.Trim().ToLower();

            return await _context.Categories.AnyAsync(c =>
                c.Name.ToLower() == normalized &&
                (excludeCategoryId == null || c.CategoryId != excludeCategoryId));
        }

        public async Task<List<CategorySummary>> GetSummariesAsync(
            string? search, string? type, string? status)
        {
            var query = _context.Categories.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(c =>
                    c.Name.ToLower().Contains(term) ||
                    c.Description.ToLower().Contains(term));
            }

            if (!string.IsNullOrWhiteSpace(type))
                query = query.Where(c => c.Type == type);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            return await query
                .OrderBy(c => c.Type)
                .ThenBy(c => c.Name)
                .Select(c => new CategorySummary
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    Type = c.Type,
                    Description = c.Description,
                    Status = c.Status,
                    TicketCount = c.Tickets.Count()
                })
                .ToListAsync();
        }

        public async Task AddAsync(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }
    }
}
