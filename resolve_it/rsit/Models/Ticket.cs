namespace rsit.Models;

public class Ticket
{
    public int TicketId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Priority { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    // Foreign keys
    public int EmployeeId { get; set; }
    public int CategoryId { get; set; }
    public int DepartmentId { get; set; }

    // Navigation properties
    public User Employee { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Department Department { get; set; } = null!;

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
    public Feedback? Feedback { get; set; }
}