using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using rsit.Models;
using rsit.Services.Interfaces;
using rsit.ViewModels;

namespace rsit.Controllers;

[Authorize(Roles = "Employee")]
public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;
    private readonly UserManager<User> _userManager;

    public TicketsController(
        ITicketService ticketService,
        UserManager<User> userManager)
    {
        _ticketService = ticketService;
        _userManager = userManager;
    }

    // =========================================================
    // CREATE TICKET
    // =========================================================

    // GET: /Tickets/Create
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model =
            await _ticketService.GetCreateTicketModelAsync();

        return View(model);
    }


    // POST: /Tickets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        CreateTicketViewModel model)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        if (!ModelState.IsValid)
        {
            await _ticketService
                .LoadCreateTicketDataAsync(model);

            return View(model);
        }

        var result =
            await _ticketService.CreateTicketAsync(
                model,
                user.Id);

        if (!result.Success)
        {
            ModelState.AddModelError(
                string.Empty,
                result.Message);

            await _ticketService
                .LoadCreateTicketDataAsync(model);

            return View(model);
        }

        TempData["SuccessMessage"] =
            result.Message;

        return RedirectToAction(
            nameof(Details),
            new
            {
                id = result.TicketId
            });
    }


    // =========================================================
    // TICKET DETAILS
    // =========================================================

    // GET: /Tickets/Details/5
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var ticket =
            await _ticketService.GetTicketDetailAsync(
                id,
                user.Id);

        if (ticket == null)
        {
            return NotFound();
        }

        return View(ticket);
    }


    // =========================================================
    // VIEW ATTACHMENT
    // =========================================================

    // GET: /Tickets/ViewAttachment/1
    [HttpGet]
    public async Task<IActionResult> ViewAttachment(int id)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var attachment =
            await _ticketService.GetAttachmentAsync(
                id,
                user.Id);

        if (attachment == null)
        {
            return NotFound();
        }

        var result =
            await _ticketService.GetAttachmentFileAsync(
                attachment);

        if (!result.Success)
        {
            return NotFound(result.Message);
        }

        return PhysicalFile(
            result.PhysicalPath,
            attachment.FileType,
            enableRangeProcessing: true);
    }


    // =========================================================
    // DOWNLOAD ATTACHMENT
    // =========================================================

    // GET: /Tickets/DownloadAttachment/1
    [HttpGet]
    public async Task<IActionResult> DownloadAttachment(
        int id)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var attachment =
            await _ticketService.GetAttachmentAsync(
                id,
                user.Id);

        if (attachment == null)
        {
            return NotFound();
        }

        var result =
            await _ticketService.GetAttachmentFileAsync(
                attachment);

        if (!result.Success)
        {
            return NotFound(result.Message);
        }

        return PhysicalFile(
            result.PhysicalPath,
            attachment.FileType,
            attachment.FileName,
            enableRangeProcessing: true);
    }


    // =========================================================
    // SUBMIT FEEDBACK
    // =========================================================

    // POST: /Tickets/SubmitFeedback
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitFeedback(
        int ticketId,
        int rating,
        string? comments)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var result =
            await _ticketService.SubmitFeedbackAsync(
                ticketId,
                user.Id,
                rating,
                comments);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] =
                result.Errors.FirstOrDefault()
                ?? "Unable to submit feedback.";

            return RedirectToAction(
                nameof(Details),
                new
                {
                    id = ticketId
                });
        }

        TempData["SuccessMessage"] =
            "Thank you. Your feedback has been submitted successfully.";

        return RedirectToAction(
            nameof(Details),
            new
            {
                id = ticketId
            });
    }


    // =========================================================
    // CLOSE TICKET
    // =========================================================

    // POST: /Tickets/Close
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var result =
            await _ticketService.CloseTicketAsync(
                id,
                user.Id);

        if (!result.Succeeded)
        {
            TempData["ErrorMessage"] =
                result.Errors.FirstOrDefault()
                ?? "Unable to close the ticket.";
        }
        else
        {
            TempData["SuccessMessage"] =
                "Ticket closed successfully.";
        }

        return RedirectToAction(
            nameof(Details),
            new
            {
                id
            });
    }


    // =========================================================
    // MY TICKETS
    // =========================================================

    // GET: /Tickets/MyTickets
    [HttpGet]
    public async Task<IActionResult> MyTickets()
    {
        var user =
            await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return Challenge();
        }

        var tickets =
            await _ticketService
                .GetTicketsForEmployeeAsync(user.Id);

        var model =
            new MyTicketsViewModel
            {
                Tickets = tickets
            };

        return View(model);
    }
}