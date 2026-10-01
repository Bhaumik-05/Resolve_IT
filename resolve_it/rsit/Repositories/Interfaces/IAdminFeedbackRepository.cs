using rsit.Models;

namespace rsit.Repositories.Interfaces;

public interface IAdminFeedbackRepository
{
    Task<List<Feedback>> SearchAsync(
        string? search,
        int? rating);
}