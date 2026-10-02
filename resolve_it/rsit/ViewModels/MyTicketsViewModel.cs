using rsit.Models;

namespace rsit.ViewModels;

public class MyTicketsViewModel
{
    public List<Ticket> Tickets { get; set; } = [];

    public int TotalTickets =>
        Tickets.Count;

    public int OpenTickets =>
        Tickets.Count(t =>
            t.Status != TicketStatuses.Resolved &&
            t.Status != TicketStatuses.Closed);

    public int ResolvedTickets =>
        Tickets.Count(t =>
            t.Status == TicketStatuses.Resolved);

    public int ClosedTickets =>
        Tickets.Count(t =>
            t.Status == TicketStatuses.Closed);
}