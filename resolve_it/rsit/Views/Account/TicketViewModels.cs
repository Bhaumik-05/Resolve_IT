using Microsoft.AspNetCore.Mvc.Rendering;

namespace rsit.ViewModels.Admin;

public class AdminTicketListViewModel
{
    public string? Search { get; set; }

    public string? Status { get; set; }

    public string? Priority { get; set; }

    public int? DepartmentId { get; set; }

    public List<SelectListItem> Departments { get; set; } = new();

    public List<AdminTicketRowViewModel> Tickets { get; set; } = new();
}

public class AdminTicketRowViewModel
{
    public int TicketId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? AssignedStaffName { get; set; }

    public DateTime CreatedAt { get; set; }
}

public class AdminTicketDetailsViewModel
{
    public int TicketId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public string EmployeeEmail { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public string CategoryName { get; set; } = string.Empty;

    public List<AdminAssignmentViewModel> Assignments { get; set; } = new();

    public List<AdminAttachmentViewModel> Attachments { get; set; } = new();

    public List<AdminHistoryViewModel> History { get; set; } = new();

    public List<SelectListItem> SupportStaff { get; set; } = new();

    public int? CurrentStaffId { get; set; }
}

public class AdminAssignmentViewModel
{
    public string StaffName { get; set; } = string.Empty;

    public string StaffEmployeeId { get; set; } = string.Empty;

    public string AssignedByName { get; set; } = string.Empty;

    public DateTime AssignedAt { get; set; }
}

public class AdminAttachmentViewModel
{
    public string FileName { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    public string UploaderName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;
}

public class AdminHistoryViewModel
{
    public string OldStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public string ChangedByName { get; set; } = string.Empty;
}

public class AssignTicketViewModel
{
    public int TicketId { get; set; }

    public int StaffId { get; set; }
}