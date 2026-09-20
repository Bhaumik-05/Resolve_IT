using rsit.Models;
using System.ComponentModel.DataAnnotations;

namespace rsit.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Employee name is required.")]
        [StringLength(100, MinimumLength = 2,
        ErrorMessage = "Employee name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z][A-Za-z .]*$", ErrorMessage = "Name can only contain letters, spaces, and periods.")]
        [Display(Name = "Employee Name")]
        public string Name { get; set; } = string.Empty;


        [Required(ErrorMessage = "Employee ID is required.")]
        [StringLength(20, MinimumLength = 2,
            ErrorMessage = "Employee ID must be between 2 and 20 characters.")]
        [RegularExpression(@"^[A-Z]{2,4}\d{3,6}$", ErrorMessage = "Employee ID format is invalid (e.g. EMP1023).")]
        [Display(Name = "Employee ID")]
        public string EmployeeId { get; set; } = string.Empty;


        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;


        [Required(ErrorMessage = "Mobile number is required.")]
        [Phone(ErrorMessage = "Enter a valid mobile number.")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "Enter a valid 10-digit mobile number.")]
        [Display(Name = "Mobile Number")]
        public string Mobile { get; set; } = string.Empty;


        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 8,
            ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$",
    ErrorMessage = "Password must include an uppercase letter, lowercase letter, digit, and special character.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;


        [Required(ErrorMessage = "Please select a department.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid department.")]
        [Display(Name = "Department")]
        public int DepartmentId { get; set; }

        public List<Department> Departments { get; set; } = new();
    }
}
