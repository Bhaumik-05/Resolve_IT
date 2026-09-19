using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.SupportStaff)]
public class SupportController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }
}