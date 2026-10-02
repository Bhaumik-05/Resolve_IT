using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using rsit.Models;

namespace rsit.ViewModels;

public class CreateTicketViewModel
{
    [Required]
    [Display(Name = "Title")]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Description")]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Priority")]
    public string Priority { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Category")]
    public int CategoryId { get; set; }

    [Required]
    [Display(Name = "Department")]
    public int DepartmentId { get; set; }

    [Display(Name = "Attachments")]
    public List<IFormFile>? Attachments { get; set; } = [];

    public string[] Priorities { get; set; } = [];

    public List<Category> Categories { get; set; } = [];

    public List<Department> Departments { get; set; } = [];
}