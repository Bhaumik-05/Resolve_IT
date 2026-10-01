namespace rsit.ViewModels;

public class AttachmentFileResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;

    public string PhysicalPath { get; set; } = string.Empty;
}