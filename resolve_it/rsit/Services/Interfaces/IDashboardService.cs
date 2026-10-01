using rsit.ViewModels.Admin;

namespace rsit.Services.Interfaces;

public interface IDashboardService
{
    Task<AdminDashboardViewModel> GetAdminDashboardAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null);
}