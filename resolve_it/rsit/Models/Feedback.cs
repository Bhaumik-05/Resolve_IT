namespace rsit.Models;

public class Feedback
{
    public int FeedbackId { get; set; }

    public int Rating { get; set; }

    public string Comments { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; }

    public int TicketId { get; set; }

    public int EmployeeId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public User Employee { get; set; } = null!;
}