using rsit.Models;
using rsit.Services;
using rsit.ViewModels;

namespace rsit.Services.Interfaces;

public interface ITicketService
{
    // =========================================================
    // CREATE TICKET
    // =========================================================

    Task<CreateTicketViewModel> GetCreateTicketModelAsync();

    Task LoadCreateTicketDataAsync(
        CreateTicketViewModel model);

    Task<TicketCreateResult> CreateTicketAsync(
        CreateTicketViewModel model,
        int employeeId);


    // =========================================================
    // TICKET DETAILS
    // =========================================================

    Task<Ticket?> GetTicketDetailAsync(
        int ticketId,
        int employeeId);


    // =========================================================
    // EMPLOYEE TICKETS
    // =========================================================

    Task<List<Ticket>> GetTicketsForEmployeeAsync(
        int employeeId);


    // =========================================================
    // EMPLOYEE FEEDBACK
    // =========================================================

    Task<ServiceResult> SubmitFeedbackAsync(
        int ticketId,
        int employeeId,
        int rating,
        string? comments);


    // =========================================================
    // EMPLOYEE TICKET CLOSURE
    // =========================================================

    Task<ServiceResult> CloseTicketAsync(
        int ticketId,
        int employeeId);


    // =========================================================
    // DROPDOWN DATA
    // =========================================================

    Task<List<Category>> GetActiveCategoriesAsync();

    Task<List<Department>> GetActiveDepartmentsAsync();


    // =========================================================
    // ATTACHMENTS
    // =========================================================

    Task<Attachment?> GetAttachmentAsync(
        int attachmentId,
        int employeeId);

    Task<AttachmentFileResult> GetAttachmentFileAsync(
        Attachment attachment);


    // =========================================================
    // SUPPORT STAFF - UPDATE STATUS
    // =========================================================

    Task<bool> UpdateTicketStatusAsync(
        int ticketId,
        int staffId,
        string newStatus);


    // =========================================================
    // SUPPORT STAFF - RESOLVE TICKET
    // =========================================================

    Task<bool> ResolveTicketAsync(
        int ticketId,
        int staffId,
        string remarks);
}