using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.Employee)]
public class EmployeeController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}