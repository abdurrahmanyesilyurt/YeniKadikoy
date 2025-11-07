using Microsoft.AspNetCore.Http;

namespace Kadikoy.DTOs;

/// <summary>
/// DTO for uploading photos to Social Responsibility news items
/// </summary>
public class SocialResponsibilityPhotoFormDto
{
    /// <summary>
    /// Image file to upload
    /// </summary>
    public IFormFile File { get; set; } = null!;

    /// <summary>
    /// Display order for the photo (optional, defaults to 0)
    /// </summary>
    public int Order { get; set; } = 0;
}

