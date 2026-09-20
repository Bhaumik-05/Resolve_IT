using rsit.ViewModels.Admin;

namespace rsit.Services.Interfaces;

public interface IUserManagementService
{
    Task<UserListViewModel> GetUserListAsync(UserListViewModel query, int currentUserId);

    /// <summary>Loads an existing user into the edit form, or null if not found.</summary>
    Task<UserFormViewModel?> GetUserForEditAsync(int userId, int currentUserId);

    /// <summary>Fills the department / role dropdowns on a form.</summary>
    Task PopulateOptionsAsync(UserFormViewModel model);

    Task<ServiceResult> CreateUserAsync(CreateUserViewModel model);

    Task<ServiceResult> UpdateUserAsync(UserFormViewModel model, int currentUserId);

    Task<ServiceResult> SetUserStatusAsync(int userId, bool activate, int currentUserId);

    Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel model);
}
