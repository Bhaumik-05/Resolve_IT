using System.ComponentModel.DataAnnotations;
using rsit.ViewModels;
using Xunit;

namespace rsit.Tests.ViewModels;

public class RegisterViewModelTests
{
    private static IList<ValidationResult> Validate(RegisterViewModel model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    private static RegisterViewModel ValidModel() => new()
    {
        Name = "Jane Doe",
        EmployeeId = "EMP1023",
        Email = "jane.doe@example.com",
        Mobile = "9876543210",
        Password = "Passw0rd!",
        ConfirmPassword = "Passw0rd!",
        DepartmentId = 1
    };

    [Fact]
    public void ValidModel_PassesValidation()
    {
        var results = Validate(ValidModel());
        Assert.Empty(results);
    }

    [Theory]
    [InlineData("EMP1023", true)]   // valid
    [InlineData("emp1023", false)]  // lowercase letters not allowed
    [InlineData("12345", false)]    // must start with letters
    [InlineData("EMP", false)]      // no digits
    [InlineData("EMP!!23", false)]  // symbols not allowed
    public void EmployeeId_RegexValidation(string employeeId, bool expectedValid)
    {
        var model = ValidModel();
        model.EmployeeId = employeeId;

        var results = Validate(model);

        Assert.Equal(expectedValid, results.All(r => !r.MemberNames.Contains(nameof(model.EmployeeId))));
    }

    [Theory]
    [InlineData("9876543210", true)]  // valid 10-digit starting 6-9
    [InlineData("1234567890", false)] // starts with invalid digit
    [InlineData("98765432", false)]   // too short
    [InlineData("98765432101", false)]// too long
    [InlineData("abcdefghij", false)] // non-numeric
    public void Mobile_RegexValidation(string mobile, bool expectedValid)
    {
        var model = ValidModel();
        model.Mobile = mobile;

        var results = Validate(model);

        Assert.Equal(expectedValid, results.All(r => !r.MemberNames.Contains(nameof(model.Mobile))));
    }

    [Theory]
    [InlineData("Passw0rd!", true)]   // has upper, lower, digit, special
    [InlineData("password", false)]   // no upper/digit/special
    [InlineData("PASSWORD1", false)]  // no lower/special
    [InlineData("Password", false)]   // no digit/special
    [InlineData("Passw0rd", false)]   // no special char
    public void Password_RegexValidation(string password, bool expectedValid)
    {
        var model = ValidModel();
        model.Password = password;
        model.ConfirmPassword = password;

        var results = Validate(model);

        Assert.Equal(expectedValid, results.All(r => !r.MemberNames.Contains(nameof(model.Password))));
    }

    [Theory]
    [InlineData("Jane Doe", true)]
    [InlineData("Jane99", false)]     // digits not allowed
    [InlineData("Jane_Doe", false)]   // underscore not allowed
    [InlineData("O'Malley", false)]   // apostrophe not allowed by this pattern (adjust if needed)
    public void Name_RegexValidation(string name, bool expectedValid)
    {
        var model = ValidModel();
        model.Name = name;

        var results = Validate(model);

        Assert.Equal(expectedValid, results.All(r => !r.MemberNames.Contains(nameof(model.Name))));
    }

    [Fact]
    public void MismatchedPasswords_FailsCompareValidation()
    {
        var model = ValidModel();
        model.ConfirmPassword = "Different1!";

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(model.ConfirmPassword)));
    }

    [Fact]
    public void InvalidEmail_FailsValidation()
    {
        var model = ValidModel();
        model.Email = "not-an-email";

        var results = Validate(model);

        Assert.Contains(results, r => r.MemberNames.Contains(nameof(model.Email)));
    }
}