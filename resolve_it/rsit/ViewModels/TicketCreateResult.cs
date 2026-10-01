namespace rsit.ViewModels;

public class TicketCreateResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public int TicketId { get; set; }
}