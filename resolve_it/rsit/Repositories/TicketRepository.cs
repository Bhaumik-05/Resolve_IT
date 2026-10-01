using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories.Interfaces;

namespace rsit.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly ApplicationDbContext _context;

    public TicketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // TICKET CREATION
    // =========================================================

    public async Task AddTicketAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
    }


    // =========================================================
    // GET TICKET DETAILS
    // =========================================================

    public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.Employee)

            .Include(t => t.Attachments)

            .Include(t => t.Assignments)
                .ThenInclude(a => a.Staff)

            .Include(t => t.History)

            .Include(t => t.Feedback)

            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    }


    // =========================================================
    // GET EMPLOYEE TICKETS
    // =========================================================

    public async Task<List<Ticket>> GetTicketsForEmployeeAsync(
        int employeeId)
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Department)
            .Include(t => t.Attachments)
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }


    // =========================================================
    // ATTACHMENTS
    // =========================================================

    public async Task AddAttachmentAsync(Attachment attachment)
    {
        await _context.Attachments.AddAsync(attachment);
    }


    public async Task<Attachment?> GetAttachmentByIdAsync(
        int attachmentId)
    {
        return await _context.Attachments
            .FirstOrDefaultAsync(a =>
                a.AttachmentId == attachmentId);
    }


    // =========================================================
    // ACTIVE CATEGORIES
    // =========================================================

    public async Task<List<Category>> GetActiveCategoriesAsync()
    {
        return await _context.Categories
            .Where(c => c.Status == RecordStatus.Active)
            .OrderBy(c => c.Name)
            .ToListAsync();
    }


    // =========================================================
    // ACTIVE DEPARTMENTS
    // =========================================================

    public async Task<List<Department>> GetActiveDepartmentsAsync()
    {
        return await _context.Departments
            .Where(d => d.Status == RecordStatus.Active)
            .OrderBy(d => d.Name)
            .ToListAsync();
    }


    // =========================================================
    // SAVE CHANGES
    // =========================================================

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}