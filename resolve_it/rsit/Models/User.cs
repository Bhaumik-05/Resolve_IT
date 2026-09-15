namespace rsit.Models;

public class User
{
    public int UserId { get; set; }

    public string EmployeeId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Mobile { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = string.Empty;

    public string AccountStatus { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    // Department relationship
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    // Navigation properties
    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    public ICollection<Assignment> AssignmentsCreated { get; set; } = new List<Assignment>();
    public ICollection<TicketHistory> TicketHistories { get; set; } = new List<TicketHistory>();
    public ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
}