namespace rsit.Models;

public class TicketHistory
{
    public int TicketHistoryId { get; set; }

    public string OldStatus { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public string Remarks { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public int TicketId { get; set; }

    public int ChangedBy { get; set; }

    public Ticket Ticket { get; set; } = null!;

    public User ChangedByUser { get; set; } = null!;
}