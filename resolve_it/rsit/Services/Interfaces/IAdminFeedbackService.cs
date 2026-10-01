using rsit.ViewModels.Admin;

namespace rsit.Services.Interfaces;

public interface IAdminFeedbackService
{
    Task<AdminFeedbackListViewModel> GetListAsync(
        AdminFeedbackListViewModel query);
}