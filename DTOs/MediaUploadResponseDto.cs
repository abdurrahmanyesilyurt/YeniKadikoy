namespace Kadikoy.DTOs;

public class MediaUploadResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? FileName { get; set; }
    public string? FileUrl { get; set; }
    public long? FileSizeBytes { get; set; }
    public string? ContentType { get; set; }
    public DateTime UploadedAt { get; set; }
}

