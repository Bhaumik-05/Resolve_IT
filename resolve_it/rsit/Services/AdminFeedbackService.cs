using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Services;

public class AdminFeedbackService : IAdminFeedbackService
{
    private readonly IAdminFeedbackRepository _repository;

    public AdminFeedbackService(
        IAdminFeedbackRepository repository)
    {
        _repository = repository;
    }

    public async Task<AdminFeedbackListViewModel> GetListAsync(
        AdminFeedbackListViewModel query)
    {
        var feedbacks = await _repository.SearchAsync(
            query.Search,
            query.Rating);

        query.Feedbacks = feedbacks
            .Select(f => new AdminFeedbackRowViewModel
            {
                FeedbackId = f.FeedbackId,
                TicketId = f.TicketId,
                TicketTitle = f.Ticket.Title,
                EmployeeName = f.Employee.Name,
                EmployeeId = f.Employee.EmployeeId,
                Rating = f.Rating,
                Comments = f.Comments,
                SubmittedAt = f.SubmittedAt
            })
            .ToList();

        return query;
    }
}