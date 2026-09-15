namespace rsit.Models;

public class Assignment
{
    public int AssignmentId { get; set; }

    public DateTime AssignedAt { get; set; }

    public int TicketId { get; set; }

    public int StaffId { get; set; }

    public int AssignedBy { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public User Staff { get; set; } = null!;

    public User AssignedByUser { get; set; } = null!;
}