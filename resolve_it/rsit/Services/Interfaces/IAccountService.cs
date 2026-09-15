using rsit.Services;
using rsit.ViewModels;

namespace rsit.Services.Interfaces;

public interface IAccountService
{
    Task<ServiceResult> RegisterAsync(RegisterViewModel model);

    Task<ServiceResult> LoginAsync(LoginViewModel model);

    Task LogoutAsync();
}