namespace rsit.ViewModels.Support;

public class SupportDashboardViewModel
{
    public string StaffName { get; set; } = string.Empty;
    public string StaffEmployeeId { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;

    public int AssignedTickets { get; set; }
    public int NewTickets { get; set; }
    public int InProgressTickets { get; set; }
    public int ResolvedTickets { get; set; }
    public int CriticalTickets { get; set; }

    public List<SupportTicketRowViewModel> RecentTickets { get; set; } = new();
}

public class SupportTicketRowViewModel
{
    public int TicketId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}