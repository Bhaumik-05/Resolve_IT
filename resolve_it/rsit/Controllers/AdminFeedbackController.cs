using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
using rsit.Services.Interfaces;
using rsit.ViewModels.Admin;

namespace rsit.Controllers;

[Authorize(Roles = UserRoles.Admin)]
[Route("Admin/Feedback")]
public class AdminFeedbackController : Controller
{
    private readonly IAdminFeedbackService _feedbackService;

    public AdminFeedbackController(
        IAdminFeedbackService feedbackService)
    {
        _feedbackService = feedbackService;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(
        [FromQuery] AdminFeedbackListViewModel query)
    {
        var model = await _feedbackService.GetListAsync(query);

        return View(model);
    }
}