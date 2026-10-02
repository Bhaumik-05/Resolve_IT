using Microsoft.AspNetCore.Hosting;
using rsit.Models;
using rsit.Repositories.Interfaces;
using rsit.Services.Interfaces;
using rsit.ViewModels;

namespace rsit.Services;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IWebHostEnvironment _environment;

    private const int MaxAttachments = 5;

    private const long MaxFileSize =
        5 * 1024 * 1024; // 5 MB

    private static readonly string[] AllowedExtensions =
    {
        ".jpg",
        ".jpeg",
        ".png",
        ".pdf",
        ".docx",
        ".xlsx"
    };


    public TicketService(
        ITicketRepository ticketRepository,
        IWebHostEnvironment environment)
    {
        _ticketRepository = ticketRepository;
        _environment = environment;
    }


    // =========================================================
    // CREATE TICKET MODEL
    // =========================================================

    public async Task<CreateTicketViewModel>
        GetCreateTicketModelAsync()
    {
        var model =
            new CreateTicketViewModel();

        await LoadCreateTicketDataAsync(model);

        return model;
    }


    // =========================================================
    // LOAD CREATE TICKET DATA
    // =========================================================

    public async Task LoadCreateTicketDataAsync(
        CreateTicketViewModel model)
    {
        model.Priorities =
            TicketPriorities.All;

        model.Categories =
            await _ticketRepository
                .GetActiveCategoriesAsync();

        model.Departments =
            await _ticketRepository
                .GetActiveDepartmentsAsync();
    }


    // =========================================================
    // CREATE TICKET
    // =========================================================

    public async Task<TicketCreateResult>
        CreateTicketAsync(
            CreateTicketViewModel model,
            int employeeId)
    {
        // -----------------------------------------------------
        // Validate priority
        // -----------------------------------------------------

        if (!TicketPriorities.All.Contains(
                model.Priority))
        {
            return new TicketCreateResult
            {
                Success = false,

                Message =
                    "Invalid ticket priority."
            };
        }


        // -----------------------------------------------------
        // Validate category
        // -----------------------------------------------------

        var categories =
            await _ticketRepository
                .GetActiveCategoriesAsync();

        var categoryExists =
            categories.Any(c =>
                c.CategoryId ==
                model.CategoryId);

        if (!categoryExists)
        {
            return new TicketCreateResult
            {
                Success = false,

                Message =
                    "Please select a valid category."
            };
        }


        // -----------------------------------------------------
        // Validate department
        // -----------------------------------------------------

        var departments =
            await _ticketRepository
                .GetActiveDepartmentsAsync();

        var departmentExists =
            departments.Any(d =>
                d.DepartmentId ==
                model.DepartmentId);

        if (!departmentExists)
        {
            return new TicketCreateResult
            {
                Success = false,

                Message =
                    "Please select a valid department."
            };
        }


        // -----------------------------------------------------
        // Validate attachments
        // -----------------------------------------------------

        var attachments =
            model.Attachments ?? [];


        if (attachments.Count >
            MaxAttachments)
        {
            return new TicketCreateResult
            {
                Success = false,

                Message =
                    $"You can upload a maximum of {MaxAttachments} files."
            };
        }


        foreach (var file in attachments)
        {
            if (file == null ||
                file.Length == 0)
            {
                continue;
            }


            // File size
            if (file.Length >
                MaxFileSize)
            {
                return new TicketCreateResult
                {
                    Success = false,

                    Message =
                        $"The file '{file.FileName}' exceeds the 5 MB limit."
                };
            }


            // File extension
            var extension =
                Path.GetExtension(
                    file.FileName)
                    .ToLowerInvariant();


            if (!AllowedExtensions.Contains(
                    extension))
            {
                return new TicketCreateResult
                {
                    Success = false,

                    Message =
                        $"The file type '{extension}' is not allowed."
                };
            }
        }


        // -----------------------------------------------------
        // Create ticket
        // -----------------------------------------------------

        var ticket = new Ticket
        {
            Title =
                model.Title.Trim(),

            Description =
                model.Description.Trim(),

            Priority =
                model.Priority,

            Status =
                TicketStatuses.New,

            CreatedAt =
                DateTime.UtcNow,

            EmployeeId =
                employeeId,

            CategoryId =
                model.CategoryId,

            DepartmentId =
                model.DepartmentId
        };


        await _ticketRepository
            .AddTicketAsync(ticket);


        // Save first so SQL Server
        // generates TicketId.
        await _ticketRepository
            .SaveChangesAsync();


        // -----------------------------------------------------
        // Save attachments
        // -----------------------------------------------------

        if (attachments.Count > 0)
        {
            var uploadDirectory =
                Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "tickets",
                    ticket.TicketId.ToString());


            Directory.CreateDirectory(
                uploadDirectory);


            foreach (var file in attachments)
            {
                if (file == null ||
                    file.Length == 0)
                {
                    continue;
                }


                var extension =
                    Path.GetExtension(
                        file.FileName)
                        .ToLowerInvariant();


                // Generate safe storage filename.
                var storedFileName =
                    $"{Guid.NewGuid():N}{extension}";


                var physicalPath =
                    Path.Combine(
                        uploadDirectory,
                        storedFileName);


                // Save physical file.
                await using (
                    var stream =
                        new FileStream(
                            physicalPath,
                            FileMode.Create))
                {
                    await file.CopyToAsync(
                        stream);
                }


                // Save attachment information.
                var attachment =
                    new Attachment
                    {
                        FileName =
                            Path.GetFileName(
                                file.FileName),

                        FilePath =
                            Path.Combine(
                                "uploads",
                                "tickets",
                                ticket.TicketId.ToString(),
                                storedFileName),

                        FileType =
                            string.IsNullOrWhiteSpace(
                                file.ContentType)
                                ? "application/octet-stream"
                                : file.ContentType,

                        FileSize =
                            file.Length,

                        UploadedAt =
                            DateTime.UtcNow,

                        TicketId =
                            ticket.TicketId,

                        UploadedBy =
                            employeeId
                    };


                await _ticketRepository
                    .AddAttachmentAsync(
                        attachment);
            }


            await _ticketRepository
                .SaveChangesAsync();
        }


        // -----------------------------------------------------
        // Success
        // -----------------------------------------------------

        return new TicketCreateResult
        {
            Success = true,

            TicketId =
                ticket.TicketId,

            Message =
                $"Ticket #{ticket.TicketId} created successfully."
        };
    }


    // =========================================================
    // GET TICKET DETAILS
    // =========================================================

    public async Task<Ticket?>
        GetTicketDetailAsync(
            int ticketId,
            int employeeId)
    {
        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(
                    ticketId);


        if (ticket == null)
        {
            return null;
        }


        // Employee can only view
        // their own tickets.
        if (ticket.EmployeeId !=
            employeeId)
        {
            return null;
        }


        return ticket;
    }


    // =========================================================
    // GET EMPLOYEE TICKETS
    // =========================================================

    public async Task<List<Ticket>>
        GetTicketsForEmployeeAsync(
            int employeeId)
    {
        return await _ticketRepository
            .GetTicketsForEmployeeAsync(
                employeeId);
    }


    // =========================================================
    // SUBMIT EMPLOYEE FEEDBACK
    // =========================================================

    public async Task<ServiceResult>
        SubmitFeedbackAsync(
            int ticketId,
            int employeeId,
            int rating,
            string? comments)
    {
        // -----------------------------------------------------
        // Validate rating
        // -----------------------------------------------------

        if (rating < 1 ||
            rating > 5)
        {
            return ServiceResult.Failure(
                "Please select a rating between 1 and 5.");
        }


        // -----------------------------------------------------
        // Get ticket
        // -----------------------------------------------------

        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(
                    ticketId);


        if (ticket == null ||
            ticket.EmployeeId != employeeId)
        {
            return ServiceResult.Failure(
                "Ticket not found.");
        }


        // -----------------------------------------------------
        // Feedback only after resolution
        // -----------------------------------------------------

        if (ticket.Status !=
                TicketStatuses.Resolved &&
            ticket.Status !=
                TicketStatuses.Closed)
        {
            return ServiceResult.Failure(
                "Feedback can only be submitted after the ticket is resolved.");
        }


        // -----------------------------------------------------
        // Only one feedback per ticket
        // -----------------------------------------------------

        if (ticket.Feedback != null)
        {
            return ServiceResult.Failure(
                "Feedback has already been submitted for this ticket.");
        }


        // -----------------------------------------------------
        // Validate comments
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
                comments) &&
            comments.Trim().Length > 1000)
        {
            return ServiceResult.Failure(
                "Feedback comments cannot exceed 1000 characters.");
        }


        // -----------------------------------------------------
        // Create feedback
        // -----------------------------------------------------

        var feedback =
            new Feedback
            {
                TicketId =
                    ticket.TicketId,

                EmployeeId =
                    employeeId,

                Rating =
                    rating,

                Comments =
                    string.IsNullOrWhiteSpace(
                        comments)
                        ? string.Empty
                        : comments.Trim(),

                SubmittedAt =
                    DateTime.UtcNow
            };


        await _ticketRepository
            .AddFeedbackAsync(
                feedback);


        await _ticketRepository
            .SaveChangesAsync();


        return ServiceResult.Success();
    }


    // =========================================================
    // CLOSE EMPLOYEE TICKET
    // =========================================================

    public async Task<ServiceResult>
        CloseTicketAsync(
            int ticketId,
            int employeeId)
    {
        // -----------------------------------------------------
        // Get ticket
        // -----------------------------------------------------

        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(
                    ticketId);


        if (ticket == null ||
            ticket.EmployeeId != employeeId)
        {
            return ServiceResult.Failure(
                "Ticket not found.");
        }


        // -----------------------------------------------------
        // Already closed
        // -----------------------------------------------------

        if (ticket.Status ==
            TicketStatuses.Closed)
        {
            return ServiceResult.Failure(
                "This ticket is already closed.");
        }


        // -----------------------------------------------------
        // Only resolved tickets can be closed
        // -----------------------------------------------------

        if (ticket.Status !=
            TicketStatuses.Resolved)
        {
            return ServiceResult.Failure(
                "Only resolved tickets can be closed.");
        }


        // -----------------------------------------------------
        // Update ticket
        // -----------------------------------------------------

        var oldStatus =
            ticket.Status;


        ticket.Status =
            TicketStatuses.Closed;


        ticket.ClosedAt =
            DateTime.UtcNow;


        // -----------------------------------------------------
        // Record closure in history
        // -----------------------------------------------------

        await _ticketRepository
            .AddHistoryAsync(
                new TicketHistory
                {
                    TicketId =
                        ticket.TicketId,

                    OldStatus =
                        oldStatus,

                    NewStatus =
                        TicketStatuses.Closed,

                    Remarks =
                        "Ticket closed by employee.",

                    ChangedAt =
                        DateTime.UtcNow,

                    ChangedBy =
                        employeeId
                });


        await _ticketRepository
            .SaveChangesAsync();


        return ServiceResult.Success();
    }


    // =========================================================
    // GET ACTIVE CATEGORIES
    // =========================================================

    public async Task<List<Category>>
        GetActiveCategoriesAsync()
    {
        return await _ticketRepository
            .GetActiveCategoriesAsync();
    }


    // =========================================================
    // GET ACTIVE DEPARTMENTS
    // =========================================================

    public async Task<List<Department>>
        GetActiveDepartmentsAsync()
    {
        return await _ticketRepository
            .GetActiveDepartmentsAsync();
    }


    // =========================================================
    // GET ATTACHMENT
    // =========================================================

    public async Task<Attachment?>
        GetAttachmentAsync(
            int attachmentId,
            int employeeId)
    {
        var attachment =
            await _ticketRepository
                .GetAttachmentByIdAsync(
                    attachmentId);


        if (attachment == null)
        {
            return null;
        }


        // Get ticket to verify ownership.
        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(
                    attachment.TicketId);


        if (ticket == null)
        {
            return null;
        }


        // Employee can only access
        // attachments belonging to
        // their own ticket.
        if (ticket.EmployeeId !=
            employeeId)
        {
            return null;
        }


        return attachment;
    }


    // =========================================================
    // GET ATTACHMENT FILE
    // =========================================================

    public async Task<AttachmentFileResult>
        GetAttachmentFileAsync(
            Attachment attachment)
    {
        if (string.IsNullOrWhiteSpace(
                attachment.FilePath))
        {
            return new AttachmentFileResult
            {
                Success = false,

                Message =
                    "Attachment path is invalid."
            };
        }


        var relativePath =
            attachment.FilePath
                .Replace(
                    '/',
                    Path.DirectorySeparatorChar);


        var physicalPath =
            Path.Combine(
                _environment.WebRootPath,
                relativePath);


        if (!File.Exists(
                physicalPath))
        {
            return new AttachmentFileResult
            {
                Success = false,

                Message =
                    "Attachment file was not found."
            };
        }


        return new AttachmentFileResult
        {
            Success = true,

            PhysicalPath =
                physicalPath,

            Message =
                "Attachment loaded successfully."
        };
    }


    // =========================================================
    // SUPPORT STAFF - UPDATE STATUS
    // =========================================================

    public async Task<bool>
        UpdateTicketStatusAsync(
            int ticketId,
            int staffId,
            string newStatus)
    {
        // -----------------------------------------------------
        // Get ticket
        // -----------------------------------------------------

        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(
                    ticketId);


        if (ticket == null)
        {
            return false;
        }


        // -----------------------------------------------------
        // Verify staff assignment
        // -----------------------------------------------------

        var isAssigned =
            ticket.Assignments
                .Any(a =>
                    a.StaffId == staffId);


        if (!isAssigned)
        {
            return false;
        }


        // -----------------------------------------------------
        // Only In Progress is allowed
        // -----------------------------------------------------

        if (newStatus !=
            TicketStatuses.InProgress)
        {
            return false;
        }


        // -----------------------------------------------------
        // Only New or Assigned tickets
        // can move to In Progress
        // -----------------------------------------------------

        if (ticket.Status !=
                TicketStatuses.New &&
            ticket.Status !=
                TicketStatuses.Assigned)
        {
            return false;
        }


        // -----------------------------------------------------
        // Store old status
        // -----------------------------------------------------

        var oldStatus =
            ticket.Status;


        // -----------------------------------------------------
        // Update status
        // -----------------------------------------------------

        ticket.Status =
            TicketStatuses.InProgress;


        // -----------------------------------------------------
        // Add ticket history
        // -----------------------------------------------------

        var history =
            new TicketHistory
            {
                TicketId =
                    ticket.TicketId,

                OldStatus =
                    oldStatus,

                NewStatus =
                    TicketStatuses.InProgress,

                Remarks =
                    "Ticket moved to In Progress by support staff.",

                ChangedAt =
                    DateTime.UtcNow,

                ChangedBy =
                    staffId
            };


        ticket.History.Add(
            history);


        // -----------------------------------------------------
        // Save changes
        // -----------------------------------------------------

        await _ticketRepository
            .SaveChangesAsync();


        return true;
    }


    // =========================================================
    // SUPPORT STAFF - RESOLVE TICKET
    // =========================================================

    public async Task<bool>
        ResolveTicketAsync(
            int ticketId,
            int staffId,
            string remarks)
    {
        // -----------------------------------------------------
        // Get ticket
        // -----------------------------------------------------

        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(
                    ticketId);


        if (ticket == null)
        {
            return false;
        }


        // -----------------------------------------------------
        // Validate resolution remarks
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                remarks))
        {
            return false;
        }


        // -----------------------------------------------------
        // Verify staff assignment
        // -----------------------------------------------------

        var isAssigned =
            ticket.Assignments
                .Any(a =>
                    a.StaffId == staffId);


        if (!isAssigned)
        {
            return false;
        }


        // -----------------------------------------------------
        // Only In Progress tickets can be resolved
        // -----------------------------------------------------

        if (ticket.Status !=
            TicketStatuses.InProgress)
        {
            return false;
        }


        // -----------------------------------------------------
        // Store old status
        // -----------------------------------------------------

        var oldStatus =
            ticket.Status;


        // -----------------------------------------------------
        // Update ticket
        // -----------------------------------------------------

        ticket.Status =
            TicketStatuses.Resolved;

        ticket.ResolvedAt =
            DateTime.UtcNow;


        // -----------------------------------------------------
        // Add ticket history
        // -----------------------------------------------------

        var history =
            new TicketHistory
            {
                TicketId =
                    ticket.TicketId,

                OldStatus =
                    oldStatus,

                NewStatus =
                    TicketStatuses.Resolved,

                Remarks =
                    remarks.Trim(),

                ChangedAt =
                    DateTime.UtcNow,

                ChangedBy =
                    staffId
            };


        ticket.History.Add(
            history);


        // -----------------------------------------------------
        // Save changes
        // -----------------------------------------------------

        await _ticketRepository
            .SaveChangesAsync();


        return true;
    }
}