using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Services;

/// <summary>FR-17: admin management of user accounts.</summary>
public class UserManagementService : IUserManagementService
{
    private const int PageSize = 10;

    private readonly UserManager<User> _userManager;
    private readonly IUserRepository _userRepository;
    private readonly IDepartmentRepository _departmentRepository;

    public UserManagementService(
        UserManager<User> userManager,
        IUserRepository userRepository,
        IDepartmentRepository departmentRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _departmentRepository = departmentRepository;
    }

    // =========================================================
    // LIST
    // =========================================================

    public async Task<UserListViewModel> GetUserListAsync(UserListViewModel query, int currentUserId)
    {
        query.PageSize = PageSize;

        var page = Math.Max(1, query.Page);

        var (users, total) = await _userRepository.SearchAsync(
            query.Search, query.DepartmentId, query.Status, query.Role, page, PageSize);

        // Requested page is past the end (e.g. after deleting/filtering): fall back to the last page.
        var lastPage = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize));
        if (page > lastPage)
        {
            page = lastPage;
            (users, total) = await _userRepository.SearchAsync(
                query.Search, query.DepartmentId, query.Status, query.Role, page, PageSize);
        }

        var roles = await _userRepository.GetRolesByUserIdsAsync(users.Select(u => u.Id));

        query.Page = page;
        query.TotalCount = total;
        query.Users = users.Select(u => new UserRowViewModel
        {
            Id = u.Id,
            EmployeeId = u.EmployeeId,
            Name = u.Name,
            Email = u.Email ?? string.Empty,
            Mobile = u.PhoneNumber ?? string.Empty,
            Department = u.Department?.Name ?? "-",
            Role = roles.TryGetValue(u.Id, out var role) ? role : string.Empty,
            AccountStatus = u.AccountStatus,
            CreatedAt = u.CreatedAt,
            IsCurrentUser = u.Id == currentUserId
        }).ToList();

        var departments = await _departmentRepository.GetActiveOrCurrentAsync(query.DepartmentId);
        query.Departments = departments
            .Select(d => new SelectListItem(d.Name, d.DepartmentId.ToString()))
            .ToList();

        return query;
    }

    // =========================================================
    // EDIT FORM
    // =========================================================

    public async Task<UserFormViewModel?> GetUserForEditAsync(int userId, int currentUserId)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user == null)
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        return new UserFormViewModel
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email ?? string.Empty,
            Mobile = user.PhoneNumber ?? string.Empty,
            DepartmentId = user.DepartmentId,
            Role = roles.FirstOrDefault() ?? string.Empty,
            AccountStatus = user.AccountStatus,
            EmployeeId = user.EmployeeId,
            IsSelf = user.Id == currentUserId
        };
    }

    public async Task PopulateOptionsAsync(UserFormViewModel model)
    {
        var departments = await _departmentRepository.GetActiveOrCurrentAsync(
            model.DepartmentId > 0 ? model.DepartmentId : null);

        model.Departments = departments
            .Select(d => new SelectListItem(d.Name, d.DepartmentId.ToString()))
            .ToList();

        model.Roles = UserRoles.All
            .Select(r => new SelectListItem(UserRoles.DisplayName(r), r))
            .ToList();
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task<ServiceResult> CreateUserAsync(CreateUserViewModel model)
    {
        var email = model.Email.Trim().ToLowerInvariant();
        var role = model.Role.Trim();

        var validation = ValidateRoleAndStatus(role, model.AccountStatus);
        if (validation != null)
            return validation;

        if (await _userManager.FindByEmailAsync(email) != null)
            return ServiceResult.Failure("An account with this email already exists.");

        var department = await _departmentRepository.GetByIdAsync(model.DepartmentId);
        if (department == null)
            return ServiceResult.Failure("The selected department does not exist.");

        if (department.Status != RecordStatus.Active)
            return ServiceResult.Failure("The selected department is inactive.");

        var user = new User
        {
            UserName = email,
            Email = email,
            Name = model.Name.Trim(),
            PhoneNumber = model.Mobile.Trim(),
            EmployeeId = await GenerateEmployeeIdAsync(UserRoles.Prefix(role)),
            DepartmentId = model.DepartmentId,
            AccountStatus = model.AccountStatus,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
            return Failure(createResult);

        var roleResult = await _userManager.AddToRoleAsync(user, role);
        if (!roleResult.Succeeded)
        {
            // Don't leave an account without a role behind.
            await _userManager.DeleteAsync(user);
            return Failure(roleResult);
        }

        return ServiceResult.Success();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task<ServiceResult> UpdateUserAsync(UserFormViewModel model, int currentUserId)
    {
        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        if (user == null)
            return ServiceResult.Failure("User not found.");

        var email = model.Email.Trim().ToLowerInvariant();
        var role = model.Role.Trim();

        var validation = ValidateRoleAndStatus(role, model.AccountStatus);
        if (validation != null)
            return validation;

        var currentRoles = await _userManager.GetRolesAsync(user);

        // An admin must not lock themselves out or drop their own admin rights.
        if (user.Id == currentUserId &&
            (model.AccountStatus != RecordStatus.Active || role != UserRoles.Admin))
        {
            return ServiceResult.Failure(
                "You cannot deactivate your own account or change your own role.");
        }

        var emailOwner = await _userManager.FindByEmailAsync(email);
        if (emailOwner != null && emailOwner.Id != user.Id)
            return ServiceResult.Failure("Another account already uses this email.");

        if (model.DepartmentId != user.DepartmentId)
        {
            var department = await _departmentRepository.GetByIdAsync(model.DepartmentId);
            if (department == null)
                return ServiceResult.Failure("The selected department does not exist.");

            if (department.Status != RecordStatus.Active)
                return ServiceResult.Failure("The selected department is inactive.");
        }

        var wasActive = user.AccountStatus == RecordStatus.Active;
        var roleChanged = currentRoles.Count != 1 || currentRoles[0] != role;

        user.Name = model.Name.Trim();
        user.Email = email;
        user.UserName = email;
        user.PhoneNumber = model.Mobile.Trim();
        user.DepartmentId = model.DepartmentId;
        user.AccountStatus = model.AccountStatus;

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            return Failure(updateResult);

        if (roleChanged)
        {
            // Add the new role first so the user is never left without one.
            if (!currentRoles.Contains(role))
            {
                var addResult = await _userManager.AddToRoleAsync(user, role);
                if (!addResult.Succeeded)
                    return Failure(addResult);
            }

            var toRemove = currentRoles.Where(r => r != role).ToList();
            if (toRemove.Count > 0)
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!removeResult.Succeeded)
                    return Failure(removeResult);
            }
        }

        // Force existing sessions to sign in again so a new role / deactivation applies now.
        if (roleChanged || (wasActive && model.AccountStatus != RecordStatus.Active))
            await _userManager.UpdateSecurityStampAsync(user);

        return ServiceResult.Success();
    }

    // =========================================================
    // ACTIVATE / DEACTIVATE
    // =========================================================

    public async Task<ServiceResult> SetUserStatusAsync(int userId, bool activate, int currentUserId)
    {
        if (!activate && userId == currentUserId)
            return ServiceResult.Failure("You cannot deactivate your own account.");

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return ServiceResult.Failure("User not found.");

        user.AccountStatus = activate ? RecordStatus.Active : RecordStatus.Inactive;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
            return Failure(result);

        if (!activate)
            await _userManager.UpdateSecurityStampAsync(user);

        return ServiceResult.Success();
    }

    // =========================================================
    // RESET PASSWORD
    // =========================================================

    public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordViewModel model)
    {
        var user = await _userManager.FindByIdAsync(model.Id.ToString());
        if (user == null)
            return ServiceResult.Failure("User not found.");

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

        if (!result.Succeeded)
            return Failure(result);

        // A reset should also clear any failed-login lockout.
        await _userManager.SetLockoutEndDateAsync(user, null);
        await _userManager.ResetAccessFailedCountAsync(user);

        return ServiceResult.Success();
    }

    // =========================================================
    // HELPERS
    // =========================================================

    private static ServiceResult? ValidateRoleAndStatus(string role, string status)
    {
        if (!UserRoles.IsValid(role))
            return ServiceResult.Failure("Please select a valid role.");

        if (!RecordStatus.IsValid(status))
            return ServiceResult.Failure("Please select a valid account status.");

        return null;
    }

    private static ServiceResult Failure(IdentityResult result) =>
        ServiceResult.Failure(result.Errors.Select(e => e.Description).ToArray());

    private async Task<string> GenerateEmployeeIdAsync(string prefix)
    {
        var next = await _userRepository.GetNextSequenceAsync(prefix);
        var candidate = $"{prefix}{next:D4}";

        while (await _userRepository.EmployeeIdExistsAsync(candidate))
        {
            next++;
            candidate = $"{prefix}{next:D4}";
        }

        return candidate;
    }
}
