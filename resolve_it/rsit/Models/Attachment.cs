namespace rsit.Models;

public class Attachment
{
    public int AttachmentId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public long FileSize { get; set; }

    public DateTime UploadedAt { get; set; }

    public int TicketId { get; set; }

    public int UploadedBy { get; set; }

    // Navigation properties
    public Ticket Ticket { get; set; } = null!;

    public User Uploader { get; set; } = null!;
}