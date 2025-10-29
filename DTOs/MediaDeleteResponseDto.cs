namespace Kadikoy.DTOs;

public class MediaDeleteResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? FileName { get; set; }
    public DateTime DeletedAt { get; set; }
}

