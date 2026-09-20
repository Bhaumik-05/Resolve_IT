using Microsoft.AspNetCore.Identity;
using Moq;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Repositories.Projections;
using rsit.Services;
using rsit.Tests.Services;
using rsit.ViewModels.Admin;
using Xunit;

namespace rsit.Tests.Admin;

// =====================================================================
// FR-17 Manage Users
// =====================================================================
public class UserManagementServiceTests
{
    private readonly Mock<UserManager<User>> _userManager;
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IDepartmentRepository> _deptRepo = new();
    private readonly UserManagementService _sut;

    public UserManagementServiceTests()
    {
        _userManager = IdentityMockFactory.MockUserManager();
        _sut = new UserManagementService(_userManager.Object, _userRepo.Object, _deptRepo.Object);
    }

    private static CreateUserViewModel ValidCreate(string role = UserRoles.SupportStaff) => new()
    {
        Name = "Asha Patel",
        Email = "Asha.Patel@Example.com",
        Mobile = "9876543210",
        DepartmentId = 2,
        Role = role,
        AccountStatus = "Active",
        Password = "Passw0rd!",
        ConfirmPassword = "Passw0rd!"
    };

    private void ActiveDepartmentExists(int id = 2) =>
        _deptRepo.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(new Department { DepartmentId = id, Name = "IT", Status = "Active" });

    [Fact]
    public async Task Create_DuplicateEmail_Fails()
    {
        _userManager.Setup(m => m.FindByEmailAsync("asha.patel@example.com"))
            .ReturnsAsync(new User());

        var result = await _sut.CreateUserAsync(ValidCreate());

        Assert.False(result.Succeeded);
        _userManager.Verify(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Create_InvalidRole_Fails()
    {
        var result = await _sut.CreateUserAsync(ValidCreate(role: "Superuser"));

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Create_InactiveDepartment_Fails()
    {
        _deptRepo.Setup(r => r.GetByIdAsync(2))
            .ReturnsAsync(new Department { DepartmentId = 2, Status = "Inactive" });

        var result = await _sut.CreateUserAsync(ValidCreate());

        Assert.False(result.Succeeded);
        Assert.Contains("inactive", result.Errors[0], StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Create_Valid_GeneratesIdNormalizesEmailAndAssignsRole()
    {
        ActiveDepartmentExists();
        _userRepo.Setup(r => r.GetNextSequenceAsync("STF")).ReturnsAsync(5);
        _userRepo.Setup(r => r.EmployeeIdExistsAsync(It.IsAny<string>())).ReturnsAsync(false);

        User? created = null;
        _userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), "Passw0rd!"))
            .Callback<User, string>((u, _) => created = u)
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), UserRoles.SupportStaff))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.CreateUserAsync(ValidCreate());

        Assert.True(result.Succeeded);
        Assert.NotNull(created);
        Assert.Equal("STF0005", created!.EmployeeId);
        Assert.Equal("asha.patel@example.com", created.Email);
        Assert.Equal("asha.patel@example.com", created.UserName);
        Assert.Equal(2, created.DepartmentId);
    }

    [Fact]
    public async Task Create_RoleAssignmentFails_RemovesAccount()
    {
        ActiveDepartmentExists();
        _userRepo.Setup(r => r.GetNextSequenceAsync(It.IsAny<string>())).ReturnsAsync(1);
        _userManager.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "boom" }));

        var result = await _sut.CreateUserAsync(ValidCreate());

        Assert.False(result.Succeeded);
        _userManager.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task SetStatus_DeactivatingSelf_Fails()
    {
        var result = await _sut.SetUserStatusAsync(7, activate: false, currentUserId: 7);

        Assert.False(result.Succeeded);
        _userManager.Verify(m => m.UpdateAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task SetStatus_Deactivate_SetsInactiveAndInvalidatesSessions()
    {
        var user = new User { Id = 3, AccountStatus = "Active" };
        _userManager.Setup(m => m.FindByIdAsync("3")).ReturnsAsync(user);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.SetUserStatusAsync(3, activate: false, currentUserId: 1);

        Assert.True(result.Succeeded);
        Assert.Equal("Inactive", user.AccountStatus);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
    }

    [Fact]
    public async Task SetStatus_Activate_DoesNotTouchSecurityStamp()
    {
        var user = new User { Id = 3, AccountStatus = "Inactive" };
        _userManager.Setup(m => m.FindByIdAsync("3")).ReturnsAsync(user);
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);

        var result = await _sut.SetUserStatusAsync(3, activate: true, currentUserId: 1);

        Assert.True(result.Succeeded);
        Assert.Equal("Active", user.AccountStatus);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Update_AdminChangingOwnRole_Fails()
    {
        var user = new User { Id = 1, AccountStatus = "Active", DepartmentId = 2 };
        _userManager.Setup(m => m.FindByIdAsync("1")).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { UserRoles.Admin });

        var result = await _sut.UpdateUserAsync(new UserFormViewModel
        {
            Id = 1, Name = "Root", Email = "root@example.com", Mobile = "9876543210",
            DepartmentId = 2, Role = UserRoles.Employee, AccountStatus = "Active"
        }, currentUserId: 1);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Update_EmailUsedByAnotherUser_Fails()
    {
        var user = new User { Id = 4, AccountStatus = "Active", DepartmentId = 2 };
        _userManager.Setup(m => m.FindByIdAsync("4")).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { UserRoles.Employee });
        _userManager.Setup(m => m.FindByEmailAsync("taken@example.com"))
            .ReturnsAsync(new User { Id = 99 });

        var result = await _sut.UpdateUserAsync(new UserFormViewModel
        {
            Id = 4, Name = "Ravi Kumar", Email = "taken@example.com", Mobile = "9876543210",
            DepartmentId = 2, Role = UserRoles.Employee, AccountStatus = "Active"
        }, currentUserId: 1);

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Update_RoleChange_AddsNewRoleRemovesOldAndInvalidatesSessions()
    {
        var user = new User { Id = 4, AccountStatus = "Active", DepartmentId = 2 };
        _userManager.Setup(m => m.FindByIdAsync("4")).ReturnsAsync(user);
        _userManager.Setup(m => m.GetRolesAsync(user)).ReturnsAsync(new List<string> { UserRoles.Employee });
        _userManager.Setup(m => m.UpdateAsync(user)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.AddToRoleAsync(user, UserRoles.SupportStaff)).ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(m => m.RemoveFromRolesAsync(user, It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.UpdateUserAsync(new UserFormViewModel
        {
            Id = 4, Name = "Ravi Kumar", Email = "Ravi@Example.com", Mobile = "9876543210",
            DepartmentId = 2, Role = UserRoles.SupportStaff, AccountStatus = "Active"
        }, currentUserId: 1);

        Assert.True(result.Succeeded);
        Assert.Equal("ravi@example.com", user.Email);
        _userManager.Verify(m => m.AddToRoleAsync(user, UserRoles.SupportStaff), Times.Once);
        _userManager.Verify(m => m.RemoveFromRolesAsync(user,
            It.Is<IEnumerable<string>>(r => r.Single() == UserRoles.Employee)), Times.Once);
        _userManager.Verify(m => m.UpdateSecurityStampAsync(user), Times.Once);
    }

    [Fact]
    public async Task ResetPassword_UnknownUser_Fails()
    {
        var result = await _sut.ResetPasswordAsync(new ResetPasswordViewModel { Id = 42, NewPassword = "Passw0rd!" });

        Assert.False(result.Succeeded);
    }
}

// =====================================================================
// FR-18 Manage Departments
// =====================================================================
public class DepartmentServiceTests
{
    private readonly Mock<IDepartmentRepository> _repo = new();
    private readonly DepartmentService _sut;

    public DepartmentServiceTests() => _sut = new DepartmentService(_repo.Object);

    [Fact]
    public async Task Create_DuplicateName_Fails()
    {
        _repo.Setup(r => r.NameExistsAsync("Finance", null)).ReturnsAsync(true);

        var result = await _sut.CreateAsync(new DepartmentFormViewModel { Name = " Finance ", Status = "Active" });

        Assert.False(result.Succeeded);
        _repo.Verify(r => r.AddAsync(It.IsAny<Department>()), Times.Never);
    }

    [Fact]
    public async Task Create_InvalidStatus_Fails()
    {
        var result = await _sut.CreateAsync(new DepartmentFormViewModel { Name = "Legal", Status = "Archived" });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Create_Valid_TrimsAndStores()
    {
        Department? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Department>()))
            .Callback<Department>(d => saved = d).Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(new DepartmentFormViewModel
        {
            Name = "  Legal ", Description = "  Contracts  ", Status = "Active"
        });

        Assert.True(result.Succeeded);
        Assert.Equal("Legal", saved!.Name);
        Assert.Equal("Contracts", saved.Description);
    }

    [Fact]
    public async Task Update_NotFound_Fails()
    {
        var result = await _sut.UpdateAsync(new DepartmentFormViewModel { Id = 9, Name = "X1", Status = "Active" });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Update_ExcludesSelfWhenCheckingDuplicateName()
    {
        var dept = new Department { DepartmentId = 5, Name = "Ops", Status = "Active" };
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dept);
        _repo.Setup(r => r.NameExistsAsync("Operations", 5)).ReturnsAsync(false);

        var result = await _sut.UpdateAsync(new DepartmentFormViewModel
        {
            Id = 5, Name = "Operations", Status = "Inactive"
        });

        Assert.True(result.Succeeded);
        Assert.Equal("Operations", dept.Name);
        Assert.Equal("Inactive", dept.Status);
        _repo.Verify(r => r.UpdateAsync(dept), Times.Once);
    }

    [Fact]
    public async Task SetStatus_TogglesStatus()
    {
        var dept = new Department { DepartmentId = 5, Status = "Active" };
        _repo.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(dept);

        var result = await _sut.SetStatusAsync(5, activate: false);

        Assert.True(result.Succeeded);
        Assert.Equal("Inactive", dept.Status);
    }
}

// =====================================================================
// FR-19 Manage Complaint Categories
// =====================================================================
public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _repo = new();
    private readonly CategoryService _sut;

    public CategoryServiceTests() => _sut = new CategoryService(_repo.Object);

    [Fact]
    public async Task Create_UnknownType_Fails()
    {
        var result = await _sut.CreateAsync(new CategoryFormViewModel
        {
            Name = "Wi-Fi", Type = "Plumbing", Status = "Active"
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Create_DuplicateName_Fails()
    {
        _repo.Setup(r => r.NameExistsAsync("Wi-Fi", null)).ReturnsAsync(true);

        var result = await _sut.CreateAsync(new CategoryFormViewModel
        {
            Name = "Wi-Fi", Type = "Network", Status = "Active"
        });

        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task Create_Valid_Stores()
    {
        Category? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<Category>()))
            .Callback<Category>(c => saved = c).Returns(Task.CompletedTask);

        var result = await _sut.CreateAsync(new CategoryFormViewModel
        {
            Name = " Wi-Fi ", Type = "Network", Description = null, Status = "Active"
        });

        Assert.True(result.Succeeded);
        Assert.Equal("Wi-Fi", saved!.Name);
        Assert.Equal("Network", saved.Type);
        Assert.Equal(string.Empty, saved.Description);
    }

    [Fact]
    public async Task SetStatus_Deactivate_Updates()
    {
        var category = new Category { CategoryId = 2, Status = "Active" };
        _repo.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(category);

        var result = await _sut.SetStatusAsync(2, activate: false);

        Assert.True(result.Succeeded);
        Assert.Equal("Inactive", category.Status);
        _repo.Verify(r => r.UpdateAsync(category), Times.Once);
    }

    [Fact]
    public async Task SetStatus_NotFound_Fails()
    {
        var result = await _sut.SetStatusAsync(404, activate: true);

        Assert.False(result.Succeeded);
    }
}

// =====================================================================
// FR-20 Admin Dashboard
// =====================================================================
public class DashboardServiceTests
{
    [Fact]
    public async Task Dashboard_OrdersStatusesByLifecycleAndKeepsUnknownLast()
    {
        var repo = new Mock<IDashboardRepository>();
        repo.Setup(r => r.GetDashboardDataAsync(It.IsAny<int>())).ReturnsAsync(new DashboardData
        {
            ByStatus = new()
            {
                new LabelCount { Label = "Closed", Count = 1 },
                new LabelCount { Label = "Zzz-Custom", Count = 1 },
                new LabelCount { Label = "New", Count = 4 },
                new LabelCount { Label = "In Progress", Count = 2 }
            }
        });

        var vm = await new DashboardService(repo.Object).GetAdminDashboardAsync();

        Assert.Equal(new[] { "New", "In Progress", "Closed", "Zzz-Custom" },
            vm.Data.ByStatus.Select(s => s.Label).ToArray());
    }
}
