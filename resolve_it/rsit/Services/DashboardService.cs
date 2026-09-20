using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Repositories.Projections;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Services;

public class DashboardService : IDashboardService
{
    private const int TrendDays = 14;

    private readonly IDashboardRepository _repository;

    public DashboardService(IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminDashboardViewModel> GetAdminDashboardAsync()
    {
        var data = await _repository.GetDashboardDataAsync(TrendDays);

        // Show statuses / priorities in lifecycle order rather than alphabetically.
        data.ByStatus = OrderBy(data.ByStatus, TicketStatuses.All);
        data.ByPriority = OrderBy(data.ByPriority, TicketPriorities.All);
        data.UsersByRole = OrderBy(data.UsersByRole, UserRoles.All);

        return new AdminDashboardViewModel { Data = data };
    }

    // Known values first (in the given order); unknown values follow alphabetically.
    private static List<LabelCount> OrderBy(List<LabelCount> items, string[] order)
    {
        return items
            .OrderBy(i =>
            {
                var index = Array.IndexOf(order, i.Label);
                return index < 0 ? int.MaxValue : index;
            })
            .ThenBy(i => i.Label)
            .ToList();
    }
}
