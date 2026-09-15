using rsit.Models;

namespace rsit.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<bool> ExistsAsync(int departmentId);
        Task<List<Department>> GetAllAsync();
    }
}
