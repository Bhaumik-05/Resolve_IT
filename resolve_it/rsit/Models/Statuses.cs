namespace rsit.Models;

/// <summary>Status values shared by users, departments and categories.</summary>
public static class RecordStatus
{
    public const string Active = "Active";
    public const string Inactive = "Inactive";

    public static readonly string[] All = { Active, Inactive };

    public static bool IsValid(string? status) =>
        status != null && All.Contains(status);
}

/// <summary>Allowed complaint category types (FR-19).</summary>
public static class CategoryTypes
{
    public static readonly string[] All =
    {
        "Hardware", "Software", "Network", "Account & Access", "Facilities", "Other"
    };

    public static bool IsValid(string? type) =>
        type != null && All.Contains(type);
}

/// <summary>
/// Ticket lifecycle values used by the admin dashboard (FR-20).
/// Keep in sync with the values written by the ticket workflow.
/// </summary>
public static class TicketStatuses
{
    public const string New = "New";
    public const string Assigned = "Assigned";
    public const string InProgress = "In Progress";
    public const string Resolved = "Resolved";
    public const string Closed = "Closed";
    public const string Reopened = "Reopened";

    public static readonly string[] All =
        { New, Assigned, InProgress, Resolved, Closed, Reopened };

    /// <summary>Statuses that mean the ticket needs no further work.</summary>
    public static readonly string[] Finished = { Resolved, Closed };
}

public static class TicketPriorities
{
    public static readonly string[] All = { "Low", "Medium", "High", "Critical" };
}
