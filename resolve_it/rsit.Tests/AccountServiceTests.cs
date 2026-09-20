using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services;
using rsit.ViewModels;
using Xunit;

namespace rsit.Tests.Services;

// Helper factory: UserManager<T> and SignInManager<T> have no parameterless/interface
// constructors, so they must be mocked via their base constructor chain.
public static class IdentityMockFactory
{
    public static Mock<UserManager<User>> MockUserManager()
    {
        var store = new Mock<IUserStore<User>>();
        var mgr = new Mock<UserManager<User>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        return mgr;
    }

    public static Mock<SignInManager<User>> MockSignInManager(UserManager<User> userManager)
    {
        var contextAccessor = new Mock<IHttpContextAccessor>();
        var claimsFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
        return new Mock<SignInManager<User>>(
            userManager, contextAccessor.Object, claimsFactory.Object,
            null!, null!, null!, null!);
    }

    public static Mock<RoleManager<IdentityRole<int>>> MockRoleManager()
    {
        var store = new Mock<IRoleStore<IdentityRole<int>>>();
        return new Mock<RoleManager<IdentityRole<int>>>(
            store.Object, null!, null!, null!, null!);
    }
}

public class AccountServiceTests
{
    private readonly Mock<UserManager<User>> _userManagerMock;
    private readonly Mock<SignInManager<User>> _signInManagerMock;
    private readonly Mock<RoleManager<IdentityRole<int>>> _roleManagerMock;
    private readonly Mock<IUserRepository> _userRepoMock;
    private readonly Mock<IDepartmentRepository> _deptRepoMock;
    private readonly AccountService _sut; // system under test

    public AccountServiceTests()
    {
        _userManagerMock = IdentityMockFactory.MockUserManager();
        _signInManagerMock = IdentityMockFactory.MockSignInManager(_userManagerMock.Object);
        _roleManagerMock = IdentityMockFactory.MockRoleManager();
        _userRepoMock = new Mock<IUserRepository>();
        _deptRepoMock = new Mock<IDepartmentRepository>();

        _sut = new AccountService(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _roleManagerMock.Object,
            _userRepoMock.Object,
            _deptRepoMock.Object);
    }

    private static RegisterViewModel ValidRegisterModel() => new()
    {
        Name = "Jane Doe",
        EmployeeId = "EMP1023",
        Email = "jane.doe@example.com",
        Mobile = "9876543210",
        Password = "Passw0rd!",
        ConfirmPassword = "Passw0rd!",
        DepartmentId = 1
    };

    // ---------------- RegisterAsync ----------------

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ReturnsFailure()
    {
        var model = ValidRegisterModel();
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email))
            .ReturnsAsync(new User { Email = model.Email });

        var result = await _sut.RegisterAsync(model);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("email"));
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmployeeId_ReturnsFailure()
    {
        var model = ValidRegisterModel();
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.EmployeeIdExistsAsync(model.EmployeeId)).ReturnsAsync(true);

        var result = await _sut.RegisterAsync(model);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("employee ID"));
    }

    [Fact]
    public async Task RegisterAsync_InvalidDepartment_ReturnsFailure()
    {
        var model = ValidRegisterModel();
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.EmployeeIdExistsAsync(model.EmployeeId)).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.ExistsAsync(model.DepartmentId)).ReturnsAsync(false);

        var result = await _sut.RegisterAsync(model);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("department"));
    }

    [Fact]
    public async Task RegisterAsync_ValidInput_CreatesUserAndAssignsEmployeeRole()
    {
        var model = ValidRegisterModel();
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.EmployeeIdExistsAsync(model.EmployeeId)).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.ExistsAsync(model.DepartmentId)).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(r => r.RoleExistsAsync("Employee")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Employee"))
            .ReturnsAsync(IdentityResult.Success);

        var result = await _sut.RegisterAsync(model);

        Assert.True(result.Succeeded);
        _userManagerMock.Verify(m => m.AddToRoleAsync(It.IsAny<User>(), "Employee"), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_RoleAssignmentFails_DeletesCreatedUser()
    {
        var model = ValidRegisterModel();
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);
        _userRepoMock.Setup(r => r.EmployeeIdExistsAsync(model.EmployeeId)).ReturnsAsync(false);
        _deptRepoMock.Setup(r => r.ExistsAsync(model.DepartmentId)).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<User>(), model.Password))
            .ReturnsAsync(IdentityResult.Success);
        _roleManagerMock.Setup(r => r.RoleExistsAsync("Employee")).ReturnsAsync(true);
        _userManagerMock.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "Employee"))
            .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "role assign failed" }));

        var result = await _sut.RegisterAsync(model);

        Assert.False(result.Succeeded);
        _userManagerMock.Verify(m => m.DeleteAsync(It.IsAny<User>()), Times.Once);
    }

    // ---------------- LoginAsync ----------------

    [Fact]
    public async Task LoginAsync_UnknownEmail_ReturnsGenericFailure()
    {
        var model = new LoginViewModel { Email = "nouser@example.com", Password = "whatever" };
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);

        var result = await _sut.LoginAsync(model);

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid email or password.", result.Errors.Single());
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ReturnsFailure()
    {
        var model = new LoginViewModel { Email = "jane@example.com", Password = "Passw0rd!" };
        var user = new User { Email = model.Email, UserName = model.Email, AccountStatus = "Inactive" };
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);

        var result = await _sut.LoginAsync(model);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("inactive"));
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsSuccess()
    {
        var model = new LoginViewModel { Email = "jane@example.com", Password = "Passw0rd!" };
        var user = new User { Email = model.Email, UserName = model.Email, AccountStatus = "Active" };
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, true))
            .ReturnsAsync(SignInResult.Success);

        var result = await _sut.LoginAsync(model);

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task LoginAsync_LockedOutAccount_ReturnsLockedOutMessage()
    {
        var model = new LoginViewModel { Email = "jane@example.com", Password = "wrong" };
        var user = new User { Email = model.Email, UserName = model.Email, AccountStatus = "Active" };
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, true))
            .ReturnsAsync(SignInResult.LockedOut);

        var result = await _sut.LoginAsync(model);

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Contains("locked"));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ReturnsGenericFailure()
    {
        var model = new LoginViewModel { Email = "jane@example.com", Password = "wrong" };
        var user = new User { Email = model.Email, UserName = model.Email, AccountStatus = "Active" };
        _userManagerMock.Setup(m => m.FindByEmailAsync(model.Email)).ReturnsAsync(user);
        _signInManagerMock.Setup(s => s.PasswordSignInAsync(
                user.UserName!, model.Password, model.RememberMe, true))
            .ReturnsAsync(SignInResult.Failed);

        var result = await _sut.LoginAsync(model);

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid email or password.", result.Errors.Single());
    }
}