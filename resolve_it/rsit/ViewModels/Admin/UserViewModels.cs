using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace rsit.ViewModels.Admin
{
    /// <summary>Fields shared by the Create and Edit user forms (FR-17).</summary>
    public class UserFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Employee name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Employee name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z][A-Za-z .]*$",
            ErrorMessage = "Name can only contain letters, spaces, and periods.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a department.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        [Required(ErrorMessage = "Please select a role.")]
        public string Role { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an account status.")]
        [Display(Name = "Account Status")]
        public string AccountStatus { get; set; } = "Active";

        /// <summary>Read-only display value on the Edit page.</summary>
        [BindNever, ValidateNever]
        public string EmployeeId { get; set; } = string.Empty;

        /// <summary>True when the admin is editing their own account.</summary>
        [BindNever, ValidateNever]
        public bool IsSelf { get; set; }

        [BindNever, ValidateNever]
        public List<SelectListItem> Departments { get; set; } = new();

        [BindNever, ValidateNever]
        public List<SelectListItem> Roles { get; set; } = new();
    }

    public class CreateUserViewModel : UserFormViewModel
    {
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must include an uppercase letter, lowercase letter, digit, and special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password.")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
            ErrorMessage = "Password must include an uppercase letter, lowercase letter, digit, and special character.")]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the password.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords do not match.")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class UserRowViewModel
    {
        public int Id { get; set; }
        public string EmployeeId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string AccountStatus { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsCurrentUser { get; set; }
    }

    /// <summary>Filter inputs (bound from the query string) plus the resulting page.</summary>
    public class UserListViewModel
    {
        public string? Search { get; set; }
        public int? DepartmentId { get; set; }
        public string? Role { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;

        [BindNever]
        public int PageSize { get; set; } = 10;

        [BindNever]
        public int TotalCount { get; set; }

        [BindNever]
        public List<UserRowViewModel> Users { get; set; } = new();

        [BindNever]
        public List<SelectListItem> Departments { get; set; } = new();

        public int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalCount / (double)PageSize));
    }
}
