using rsit.ViewModels.Admin;

namespace rsit.Services.Interfaces;

public interface IAdminTicketService
{
    Task<AdminTicketListViewModel> GetListAsync(
        AdminTicketListViewModel query);

    Task<AdminTicketDetailsViewModel?> GetDetailsAsync(
        int ticketId);

    Task<ServiceResult> AssignTicketAsync(
        AssignTicketViewModel model,
        int adminId);
}