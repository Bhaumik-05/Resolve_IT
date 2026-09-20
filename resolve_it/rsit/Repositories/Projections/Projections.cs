namespace rsit.Repositories.Projections;

public class DepartmentSummary
{
    public int DepartmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public int SupportStaffCount { get; set; }
    public int TicketCount { get; set; }
}

public class CategorySummary
{
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TicketCount { get; set; }
}

public class LabelCount
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DailyCount
{
    public DateTime Date { get; set; }
    public int Count { get; set; }
}

public class RecentTicketRow
{
    public int TicketId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class DashboardData
{
    public int TotalTickets { get; set; }
    public int OpenTickets { get; set; }
    public int FinishedTickets { get; set; }
    public int UnassignedTickets { get; set; }
    public int CreatedLast7Days { get; set; }
    public double? AverageResolutionHours { get; set; }
    public double? AverageRating { get; set; }

    public List<LabelCount> ByStatus { get; set; } = new();
    public List<LabelCount> ByPriority { get; set; } = new();
    public List<LabelCount> ByDepartment { get; set; } = new();
    public List<LabelCount> ByCategory { get; set; } = new();
    public List<DailyCount> Trend { get; set; } = new();
    public List<RecentTicketRow> RecentTickets { get; set; } = new();

    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public List<LabelCount> UsersByRole { get; set; } = new();

    public int TotalDepartments { get; set; }
    public int ActiveDepartments { get; set; }
    public int TotalCategories { get; set; }
    public int ActiveCategories { get; set; }
}
