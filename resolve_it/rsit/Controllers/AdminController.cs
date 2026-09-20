using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.Admin)]

    public class AdminController : Controller
    {
    [HttpGet]
    public IActionResult Index()
        {
            return View();
        }
    }
