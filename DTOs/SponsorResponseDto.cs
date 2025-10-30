using Kadikoy.Models;

namespace Kadikoy.DTOs;

/// <summary>
/// Response DTO for Sponsor
/// </summary>
public class SponsorResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public SportType? SportType { get; set; }
    public string SportTypeName { get; set; } = string.Empty;
    public SponsorPlacement Placement { get; set; }
    public string PlacementName { get; set; } = string.Empty;
    public string? PhotoUrl { get; set; }
    public string? LogoUrl { get; set; }
    public string? WebsiteUrl { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

