using rsit.Models;
using rsit.Repositories.Projections;

namespace rsit.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<bool> ExistsAsync(int departmentId);

        /// <summary>Active departments only (used by the registration dropdown).</summary>
        Task<List<Department>> GetAllAsync();

        /// <summary>Active departments plus the given one, even if inactive.</summary>
        Task<List<Department>> GetActiveOrCurrentAsync(int? currentDepartmentId);

        Task<Department?> GetByIdAsync(int departmentId);

        Task<bool> NameExistsAsync(string name, int? excludeDepartmentId);

        Task<List<DepartmentSummary>> GetSummariesAsync(string? search, string? status);

        Task AddAsync(Department department);

        Task UpdateAsync(Department department);
    }
}
