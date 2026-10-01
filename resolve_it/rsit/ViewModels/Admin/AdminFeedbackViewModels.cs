using Microsoft.AspNetCore.Mvc.Rendering;

namespace rsit.ViewModels.Admin;

public class AdminFeedbackListViewModel
{
    public string? Search { get; set; }

    public int? Rating { get; set; }

    public List<AdminFeedbackRowViewModel> Feedbacks { get; set; } = new();
}

public class AdminFeedbackRowViewModel
{
    public int FeedbackId { get; set; }

    public int TicketId { get; set; }

    public string TicketTitle { get; set; } = string.Empty;

    public string EmployeeName { get; set; } = string.Empty;

    public string EmployeeId { get; set; } = string.Empty;

    public int Rating { get; set; }

    public string Comments { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }
}