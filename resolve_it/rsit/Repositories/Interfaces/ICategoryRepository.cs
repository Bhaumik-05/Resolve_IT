using rsit.Models;
using rsit.Repositories.Projections;

namespace rsit.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        /// <summary>Active categories only - what ticket creation should offer.</summary>
        Task<List<Category>> GetActiveAsync();

        Task<Category?> GetByIdAsync(int categoryId);

        Task<bool> NameExistsAsync(string name, int? excludeCategoryId);

        Task<List<CategorySummary>> GetSummariesAsync(string? search, string? type, string? status);

        Task AddAsync(Category category);

        Task UpdateAsync(Category category);
    }
}
