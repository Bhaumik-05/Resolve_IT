using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Repositories.Projections;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Services;

public class DashboardService : IDashboardService
{
    private const int DefaultTrendDays = 14;

    private readonly IDashboardRepository _repository;

    public DashboardService(
        IDashboardRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminDashboardViewModel>
        GetAdminDashboardAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null)
    {
        // If only one side of the range is supplied,
        // derive the other side.

        if (fromDate.HasValue &&
            toDate.HasValue &&
            fromDate > toDate)
        {
            (fromDate, toDate) =
                (toDate, fromDate);
        }

        var trendDays = DefaultTrendDays;

        if (fromDate.HasValue || toDate.HasValue)
        {
            var from =
                fromDate?.Date ??
                toDate!.Value.Date.AddDays(
                    -(DefaultTrendDays - 1));

            var to =
                toDate?.Date ??
                DateTime.UtcNow.Date;

            trendDays =
                Math.Max(
                    1,
                    (to - from).Days + 1);
        }

        var data =
            await _repository.GetDashboardDataAsync(
                trendDays,
                fromDate,
                toDate);


        // Known values first.
        data.ByStatus =
            OrderBy(
                data.ByStatus,
                TicketStatuses.All);

        data.ByPriority =
            OrderBy(
                data.ByPriority,
                TicketPriorities.All);

        data.UsersByRole =
            OrderBy(
                data.UsersByRole,
                UserRoles.All);


        return new AdminDashboardViewModel
        {
            Data = data,
            FromDate = fromDate,
            ToDate = toDate
        };
    }


    private static List<LabelCount> OrderBy(
        List<LabelCount> items,
        string[] order)
    {
        return items
            .OrderBy(i =>
            {
                var index =
                    Array.IndexOf(order, i.Label);

                return index < 0
                    ? int.MaxValue
                    : index;
            })
            .ThenBy(i => i.Label)
            .ToList();
    }
}