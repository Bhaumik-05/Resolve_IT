using rsit.Models;

namespace rsit.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByEmployeeIdAsync(string employeeId);

        Task<bool> EmployeeIdExistsAsync(string employeeId);

        Task<int> GetNextSequenceAsync(string prefix);

        /// <summary>Paged, filtered user search (Department is included).</summary>
        Task<(List<User> Items, int Total)> SearchAsync(
            string? search, int? departmentId, string? status, string? role,
            int page, int pageSize);

        /// <summary>Maps user id -> role name for the given users.</summary>
        Task<Dictionary<int, string>> GetRolesByUserIdsAsync(IEnumerable<int> userIds);
    }
}
