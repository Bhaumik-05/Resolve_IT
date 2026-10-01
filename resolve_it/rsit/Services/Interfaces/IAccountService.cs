
using rsit.Services;
using rsit.ViewModels;

namespace rsit.Services.Interfaces;

public interface IAccountService
{
    Task<ServiceResult> LoginAsync(LoginViewModel model);

    Task LogoutAsync();
}
