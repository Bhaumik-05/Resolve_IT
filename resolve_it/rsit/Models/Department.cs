namespace rsit.Models;

public class Department
{
    public int DepartmentId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public ICollection<User> Users { get; set; } = new List<User>();

    public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
}