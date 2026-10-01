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
    private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

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

    public async Task<CreateTicketViewModel> GetCreateTicketModelAsync()
    {
        var model = new CreateTicketViewModel();

        await LoadCreateTicketDataAsync(model);

        return model;
    }


    // =========================================================
    // LOAD CREATE TICKET DATA
    // =========================================================

    public async Task LoadCreateTicketDataAsync(
        CreateTicketViewModel model)
    {
        model.Priorities = TicketPriorities.All;

        model.Categories =
            await _ticketRepository.GetActiveCategoriesAsync();

        model.Departments =
            await _ticketRepository.GetActiveDepartmentsAsync();
    }


    // =========================================================
    // CREATE TICKET
    // =========================================================

    public async Task<TicketCreateResult> CreateTicketAsync(
        CreateTicketViewModel model,
        int employeeId)
    {
        // -----------------------------------------------------
        // Validate priority
        // -----------------------------------------------------

        if (!TicketPriorities.All.Contains(model.Priority))
        {
            return new TicketCreateResult
            {
                Success = false,
                Message = "Invalid ticket priority."
            };
        }


        // -----------------------------------------------------
        // Validate category
        // -----------------------------------------------------

        var categories =
            await _ticketRepository.GetActiveCategoriesAsync();

        var categoryExists =
            categories.Any(c =>
                c.CategoryId == model.CategoryId);

        if (!categoryExists)
        {
            return new TicketCreateResult
            {
                Success = false,
                Message = "Please select a valid category."
            };
        }


        // -----------------------------------------------------
        // Validate department
        // -----------------------------------------------------

        var departments =
            await _ticketRepository.GetActiveDepartmentsAsync();

        var departmentExists =
            departments.Any(d =>
                d.DepartmentId == model.DepartmentId);

        if (!departmentExists)
        {
            return new TicketCreateResult
            {
                Success = false,
                Message = "Please select a valid department."
            };
        }


        // -----------------------------------------------------
        // Validate attachments
        // -----------------------------------------------------

        var attachments = model.Attachments ?? [];

        if (attachments.Count > MaxAttachments)
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
            if (file == null || file.Length == 0)
            {
                continue;
            }

            // File size
            if (file.Length > MaxFileSize)
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
                Path.GetExtension(file.FileName)
                    .ToLowerInvariant();

            if (!AllowedExtensions.Contains(extension))
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
            Title = model.Title.Trim(),

            Description =
                model.Description.Trim(),

            Priority = model.Priority,

            Status = TicketStatuses.New,

            CreatedAt = DateTime.UtcNow,

            EmployeeId = employeeId,

            CategoryId = model.CategoryId,

            DepartmentId = model.DepartmentId
        };

        await _ticketRepository.AddTicketAsync(ticket);

        // Save first so SQL Server generates TicketId.
        await _ticketRepository.SaveChangesAsync();


        // -----------------------------------------------------
        // Save attachments
        // -----------------------------------------------------

        if (attachments.Count > 0)
        {
            var uploadDirectory = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "tickets",
                ticket.TicketId.ToString());

            Directory.CreateDirectory(uploadDirectory);


            foreach (var file in attachments)
            {
                if (file == null || file.Length == 0)
                {
                    continue;
                }

                var extension =
                    Path.GetExtension(file.FileName)
                        .ToLowerInvariant();


                // Generate a safe storage filename.
                // Original filename is NOT used for storage.
                var storedFileName =
                    $"{Guid.NewGuid():N}{extension}";


                var physicalPath =
                    Path.Combine(
                        uploadDirectory,
                        storedFileName);


                // Save physical file.
                await using (var stream =
                    new FileStream(
                        physicalPath,
                        FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }


                // Save attachment information in database.
                var attachment = new Attachment
                {
                    FileName =
                        Path.GetFileName(file.FileName),

                    FilePath = Path.Combine(
                        "uploads",
                        "tickets",
                        ticket.TicketId.ToString(),
                        storedFileName),

                    FileType =
                        string.IsNullOrWhiteSpace(
                            file.ContentType)
                            ? "application/octet-stream"
                            : file.ContentType,

                    FileSize = file.Length,

                    UploadedAt = DateTime.UtcNow,

                    TicketId = ticket.TicketId,

                    UploadedBy = employeeId
                };


                await _ticketRepository
                    .AddAttachmentAsync(attachment);
            }


            await _ticketRepository.SaveChangesAsync();
        }


        // -----------------------------------------------------
        // Success
        // -----------------------------------------------------

        return new TicketCreateResult
        {
            Success = true,

            TicketId = ticket.TicketId,

            Message =
                $"Ticket #{ticket.TicketId} created successfully."
        };
    }


    // =========================================================
    // GET TICKET DETAILS
    // =========================================================

    public async Task<Ticket?> GetTicketDetailAsync(
        int ticketId,
        int employeeId)
    {
        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(ticketId);


        if (ticket == null)
        {
            return null;
        }


        // Employee can only view their own tickets.
        if (ticket.EmployeeId != employeeId)
        {
            return null;
        }


        return ticket;
    }


    // =========================================================
    // GET EMPLOYEE TICKETS
    // =========================================================

    public async Task<List<Ticket>> GetTicketsForEmployeeAsync(
        int employeeId)
    {
        return await _ticketRepository
            .GetTicketsForEmployeeAsync(employeeId);
    }


    // =========================================================
    // GET ACTIVE CATEGORIES
    // =========================================================

    public async Task<List<Category>> GetActiveCategoriesAsync()
    {
        return await _ticketRepository
            .GetActiveCategoriesAsync();
    }


    // =========================================================
    // GET ACTIVE DEPARTMENTS
    // =========================================================

    public async Task<List<Department>> GetActiveDepartmentsAsync()
    {
        return await _ticketRepository
            .GetActiveDepartmentsAsync();
    }


    // =========================================================
    // GET ATTACHMENT
    // =========================================================

    public async Task<Attachment?> GetAttachmentAsync(
        int attachmentId,
        int employeeId)
    {
        var attachment =
            await _ticketRepository
                .GetAttachmentByIdAsync(attachmentId);


        if (attachment == null)
        {
            return null;
        }


        // Get the ticket to verify ownership.
        var ticket =
            await _ticketRepository
                .GetTicketByIdAsync(
                    attachment.TicketId);


        if (ticket == null)
        {
            return null;
        }


        // Employee can only access attachments
        // belonging to their own ticket.
        if (ticket.EmployeeId != employeeId)
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
        // -----------------------------------------------------
        // Check database path
        // -----------------------------------------------------

        if (string.IsNullOrWhiteSpace(
                attachment.FilePath))
        {
            return new AttachmentFileResult
            {
                Success = false,
                Message =
                    "Attachment path is missing."
            };
        }


        // -----------------------------------------------------
        // Normalize stored path
        // -----------------------------------------------------

        var storedPath =
            attachment.FilePath.Trim();


        storedPath = storedPath
            .Replace(
                '/',
                Path.DirectorySeparatorChar)
            .Replace(
                '\\',
                Path.DirectorySeparatorChar);


        // -----------------------------------------------------
        // Get wwwroot path
        // -----------------------------------------------------

        var webRootPath =
            Path.GetFullPath(
                _environment.WebRootPath);


        string physicalPath;


        // -----------------------------------------------------
        // Handle old/new path formats
        // -----------------------------------------------------

        /*
         * New records:
         *
         * uploads/tickets/5/file.jpg
         *
         * Old records may contain:
         *
         * /uploads/tickets/5/file.jpg
         *
         * wwwroot/uploads/tickets/5/file.jpg
         *
         * D:\...\wwwroot\uploads\tickets\5\file.jpg
         */


        // Remove leading slash/backslash.
        storedPath = storedPath.TrimStart(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);


        // Remove "wwwroot\" if it exists.
        var webRootPrefix =
            "wwwroot" +
            Path.DirectorySeparatorChar;


        if (storedPath.StartsWith(
                webRootPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            storedPath =
                storedPath.Substring(
                    webRootPrefix.Length);
        }


        // -----------------------------------------------------
        // Resolve physical path
        // -----------------------------------------------------

        if (Path.IsPathFullyQualified(
                storedPath))
        {
            // Old absolute path.
            physicalPath =
                Path.GetFullPath(
                    storedPath);
        }
        else
        {
            // Normal relative path.
            physicalPath =
                Path.GetFullPath(
                    Path.Combine(
                        webRootPath,
                        storedPath));
        }


        // -----------------------------------------------------
        // Security check
        // -----------------------------------------------------

        var webRootWithSeparator =
            webRootPath.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;


        if (!physicalPath.StartsWith(
                webRootWithSeparator,
                StringComparison.OrdinalIgnoreCase))
        {
            return new AttachmentFileResult
            {
                Success = false,
                Message =
                    "Invalid attachment path."
            };
        }


        // -----------------------------------------------------
        // Check whether file exists
        // -----------------------------------------------------

        if (!File.Exists(physicalPath))
        {
            return new AttachmentFileResult
            {
                Success = false,
                Message =
                    "Attachment file was not found."
            };
        }


        // -----------------------------------------------------
        // Return physical path
        // -----------------------------------------------------

        return new AttachmentFileResult
        {
            Success = true,
            PhysicalPath = physicalPath
        };
    }
}