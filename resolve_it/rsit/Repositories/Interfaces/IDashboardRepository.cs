using rsit.Repositories.Projections;

namespace rsit.Repositories.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardData> GetDashboardDataAsync(int trendDays);
    }
}
