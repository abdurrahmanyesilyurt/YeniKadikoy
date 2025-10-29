using Kadikoy.Models;

namespace Kadikoy.DTOs;

/// <summary>
/// Haber response DTO
/// </summary>
public class NewsResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public SportType SportType { get; set; }
    public string SportTypeName { get; set; } = string.Empty;
    public DateTime PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<NewsMediaResponseDto> MediaFiles { get; set; } = new();
}

/// <summary>
/// Haber medya response DTO
/// </summary>
public class NewsMediaResponseDto
{
    public int Id { get; set; }
    public MediaType MediaType { get; set; }
    public string MediaTypeName { get; set; } = string.Empty;
    public string S3Url { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public long FileSize { get; set; }
    public int Order { get; set; }
    public DateTime UploadedAt { get; set; }
}

