using rsit.Models;

namespace rsit.Repositories.Interfaces;

public interface IAdminTicketRepository
{
    Task<List<Ticket>> SearchAsync(
        string? search,
        string? status,
        string? priority,
        int? departmentId);

    Task<Ticket?> GetDetailsAsync(int ticketId);

    Task<List<User>> GetActiveSupportStaffAsync();

    Task<Assignment?> GetCurrentAssignmentAsync(int ticketId);

    Task AddAssignmentAsync(Assignment assignment);

    Task AddHistoryAsync(TicketHistory history);

    Task SaveChangesAsync();
}