using Microsoft.AspNetCore.Mvc.Rendering;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Services;

public class AdminTicketService : IAdminTicketService
{
    private readonly IAdminTicketRepository _repository;
    private readonly IDepartmentRepository _departmentRepository;

    public AdminTicketService(
        IAdminTicketRepository repository,
        IDepartmentRepository departmentRepository)
    {
        _repository = repository;
        _departmentRepository = departmentRepository;
    }

    public async Task<AdminTicketListViewModel> GetListAsync(
        AdminTicketListViewModel query)
    {
        var tickets = await _repository.SearchAsync(
            query.Search,
            query.Status,
            query.Priority,
            query.DepartmentId);

        query.Tickets = tickets.Select(t =>
        {
            var latestAssignment = t.Assignments
                .OrderByDescending(a => a.AssignedAt)
                .FirstOrDefault();

            return new AdminTicketRowViewModel
            {
                TicketId = t.TicketId,
                Title = t.Title,
                EmployeeName = t.Employee.Name,
                EmployeeId = t.Employee.EmployeeId,
                DepartmentName = t.Department.Name,
                CategoryName = t.Category.Name,
                Priority = t.Priority,
                Status = t.Status,
                AssignedStaffName = latestAssignment?.Staff.Name,
                CreatedAt = t.CreatedAt
            };
        }).ToList();

        var departments =
            await _departmentRepository.GetActiveOrCurrentAsync(
                query.DepartmentId);

        query.Departments = departments
            .Select(d => new SelectListItem(
                d.Name,
                d.DepartmentId.ToString()))
            .ToList();

        return query;
    }

    public async Task<AdminTicketDetailsViewModel?> GetDetailsAsync(
        int ticketId)
    {
        var ticket = await _repository.GetDetailsAsync(ticketId);

        if (ticket == null)
            return null;

        var staff = await _repository.GetActiveSupportStaffAsync();

        var currentAssignment = ticket.Assignments
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefault();

        return new AdminTicketDetailsViewModel
        {
            TicketId = ticket.TicketId,
            Title = ticket.Title,
            Description = ticket.Description,
            Priority = ticket.Priority,
            Status = ticket.Status,
            CreatedAt = ticket.CreatedAt,
            ResolvedAt = ticket.ResolvedAt,
            ClosedAt = ticket.ClosedAt,

            EmployeeName = ticket.Employee.Name,
            EmployeeId = ticket.Employee.EmployeeId,
            EmployeeEmail = ticket.Employee.Email ?? string.Empty,

            DepartmentName = ticket.Department.Name,
            CategoryName = ticket.Category.Name,

            CurrentStaffId = currentAssignment?.StaffId,

            SupportStaff = staff.Select(s =>
                new SelectListItem(
                    $"{s.Name} ({s.EmployeeId})",
                    s.Id.ToString(),
                    s.Id == currentAssignment?.StaffId))
                .ToList(),

            Assignments = ticket.Assignments
                .OrderByDescending(a => a.AssignedAt)
                .Select(a => new AdminAssignmentViewModel
                {
                    StaffName = a.Staff.Name,
                    StaffEmployeeId = a.Staff.EmployeeId,
                    AssignedByName = a.AssignedByUser.Name,
                    AssignedAt = a.AssignedAt
                })
                .ToList(),

            Attachments = ticket.Attachments
                .OrderByDescending(a => a.UploadedAt)
                .Select(a => new AdminAttachmentViewModel
                {
                    FileName = a.FileName,
                    FileType = a.FileType,
                    FileSize = a.FileSize,
                    UploadedAt = a.UploadedAt,
                    UploaderName = a.Uploader.Name,
                    FilePath = a.FilePath
                })
                .ToList(),

            History = ticket.History
                .OrderByDescending(h => h.ChangedAt)
                .Select(h => new AdminHistoryViewModel
                {
                    OldStatus = h.OldStatus,
                    NewStatus = h.NewStatus,
                    Remarks = h.Remarks,
                    ChangedAt = h.ChangedAt,
                    ChangedByName = h.ChangedByUser.Name
                })
                .ToList()
        };
    }

    public async Task<ServiceResult> AssignTicketAsync(
        AssignTicketViewModel model,
        int adminId)
    {
        var ticket = await _repository.GetDetailsAsync(model.TicketId);

        if (ticket == null)
            return ServiceResult.Failure("Ticket not found.");

        var staff = await _repository.GetActiveSupportStaffAsync();

        var selectedStaff = staff.FirstOrDefault(
            s => s.Id == model.StaffId);

        if (selectedStaff == null)
        {
            return ServiceResult.Failure(
                "Please select a valid active support staff member.");
        }

        var previousAssignment = ticket.Assignments
            .OrderByDescending(a => a.AssignedAt)
            .FirstOrDefault();

        if (previousAssignment?.StaffId == selectedStaff.Id)
        {
            return ServiceResult.Failure(
                "This ticket is already assigned to the selected staff member.");
        }

        await _repository.AddAssignmentAsync(new Assignment
        {
            TicketId = ticket.TicketId,
            StaffId = selectedStaff.Id,
            AssignedBy = adminId,
            AssignedAt = DateTime.UtcNow
        });

        var oldStatus = ticket.Status;

        if (ticket.Status == TicketStatuses.New ||
            ticket.Status == TicketStatuses.Reopened)
        {
            ticket.Status = TicketStatuses.Assigned;
        }

        if (oldStatus != ticket.Status)
        {
            await _repository.AddHistoryAsync(new TicketHistory
            {
                TicketId = ticket.TicketId,
                OldStatus = oldStatus,
                NewStatus = ticket.Status,
                Remarks = $"Ticket assigned to {selectedStaff.Name}.",
                ChangedAt = DateTime.UtcNow,
                ChangedBy = adminId
            });
        }

        await _repository.SaveChangesAsync();

        return ServiceResult.Success();
    }
}