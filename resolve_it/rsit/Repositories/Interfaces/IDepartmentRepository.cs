namespace rsit.Repositories.Interfaces
{
    public interface IDepartmentRepository
    {
        Task<bool> ExistsAsync(int departmentId);
    }
}
