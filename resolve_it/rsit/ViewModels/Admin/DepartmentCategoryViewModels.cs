using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using rsit.Repositories.Projections;

namespace rsit.ViewModels.Admin
{
    // ---------------- FR-18 Departments ----------------

    public class DepartmentFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Department name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 &,.\-/()]*$",
            ErrorMessage = "Name may contain letters, numbers, spaces and & , . - / ( ) only.")]
        [Display(Name = "Department Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a status.")]
        public string Status { get; set; } = "Active";
    }

    public class DepartmentListViewModel
    {
        public string? Search { get; set; }
        public string? Status { get; set; }

        [BindNever, ValidateNever]
        public List<DepartmentSummary> Departments { get; set; } = new();
    }

    // ---------------- FR-19 Categories ----------------

    public class CategoryFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Category name is required.")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Category name must be between 2 and 100 characters.")]
        [RegularExpression(@"^[A-Za-z0-9][A-Za-z0-9 &,.\-/()]*$",
            ErrorMessage = "Name may contain letters, numbers, spaces and & , . - / ( ) only.")]
        [Display(Name = "Category Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a category type.")]
        [Display(Name = "Category Type")]
        public string Type { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a status.")]
        public string Status { get; set; } = "Active";
    }

    public class CategoryListViewModel
    {
        public string? Search { get; set; }
        public string? Type { get; set; }
        public string? Status { get; set; }

        [BindNever, ValidateNever]
        public List<CategorySummary> Categories { get; set; } = new();
    }
}
