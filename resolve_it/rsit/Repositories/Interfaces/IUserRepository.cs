using rsit.Models;

namespace rsit.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);

        Task<User?> GetByEmployeeIdAsync(string employeeId);

        Task<bool> EmployeeIdExistsAsync(string employeeId);

        Task<int> GetNextSequenceAsync(string prefix);
    }
}
