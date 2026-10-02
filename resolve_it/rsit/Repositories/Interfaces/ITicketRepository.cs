using rsit.Models;

namespace rsit.Repositories.Interfaces;

public interface ITicketRepository
{
    // Ticket creation
    Task AddTicketAsync(Ticket ticket);


    // Ticket details
    Task<Ticket?> GetTicketByIdAsync(
        int ticketId);


    // Employee tickets
    Task<List<Ticket>> GetTicketsForEmployeeAsync(
        int employeeId);


    // Attachments
    Task AddAttachmentAsync(
        Attachment attachment);

    Task<Attachment?> GetAttachmentByIdAsync(
        int attachmentId);


    // Dropdown data
    Task<List<Category>> GetActiveCategoriesAsync();

    Task<List<Department>> GetActiveDepartmentsAsync();


    // Employee feedback
    Task AddFeedbackAsync(
        Feedback feedback);


    // Ticket history
    Task AddHistoryAsync(
        TicketHistory history);


    // Save changes
    Task SaveChangesAsync();
}