using Microsoft.EntityFrameworkCore;
using rsit.Data;
using rsit.Models;
using rsit.Repositories;
using Xunit;

namespace rsit.Tests.Admin;

public class DashboardRepositoryTests
{
    private static ApplicationDbContext NewContext() =>
        new(new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options);

    [Fact]
    public async Task EmptyDatabase_ReturnsZerosAndFullTrend()
    {
        using var context = NewContext();

        var data = await new DashboardRepository(context).GetDashboardDataAsync(14);

        Assert.Equal(0, data.TotalTickets);
        Assert.Equal(0, data.OpenTickets);
        Assert.Null(data.AverageResolutionHours);
        Assert.Null(data.AverageRating);
        Assert.Equal(14, data.Trend.Count);
        Assert.All(data.Trend, d => Assert.Equal(0, d.Count));
    }

    [Fact]
    public async Task Tickets_AreCountedByStatusOpenUnassignedAndResolutionTime()
    {
        using var context = NewContext();

        var dept = new Department { Name = "IT", Status = "Active" };
        var category = new Category { Name = "Wi-Fi", Type = "Network", Status = "Active" };
        var employee = new User { UserName = "e@x.com", Name = "Emp", EmployeeId = "EMP0001", AccountStatus = "Active", Department = dept };
        var staff = new User { UserName = "s@x.com", Name = "Staff", EmployeeId = "STF0001", AccountStatus = "Active", Department = dept };

        var now = DateTime.UtcNow;

        Ticket Make(string status, DateTime created, DateTime? resolved = null) => new()
        {
            Title = "t", Description = "d", Priority = "High", Status = status,
            CreatedAt = created, ResolvedAt = resolved,
            Employee = employee, Category = category, Department = dept
        };

        var assigned = Make("Assigned", now.AddHours(-1));
        assigned.Assignments.Add(new Assignment { Staff = staff, AssignedByUser = staff, AssignedAt = now });

        context.Tickets.AddRange(
            Make("New", now.AddHours(-2)),                                   // open, unassigned
            assigned,                                                        // open, assigned
            Make("Resolved", now.AddHours(-10), now.AddHours(-6)),           // resolved after 4h
            Make("Closed", now.AddDays(-20), now.AddDays(-19)));             // old, resolved after 24h
        await context.SaveChangesAsync();

        var data = await new DashboardRepository(context).GetDashboardDataAsync(14);

        Assert.Equal(4, data.TotalTickets);
        Assert.Equal(2, data.OpenTickets);
        Assert.Equal(2, data.FinishedTickets);
        Assert.Equal(1, data.UnassignedTickets);
        Assert.Equal(3, data.CreatedLast7Days);
        Assert.Equal(14.0, data.AverageResolutionHours!.Value, 1);
        Assert.Equal(1, data.ByStatus.Single(s => s.Label == "New").Count);
        Assert.Equal(4, data.ByDepartment.Single(d => d.Label == "IT").Count);
        Assert.Equal(4, data.RecentTickets.Count);
    }
}
