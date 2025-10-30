using System.ComponentModel.DataAnnotations;
using Kadikoy.Models;

namespace Kadikoy.DTOs;

/// <summary>
/// Create DTO for Sponsor
/// </summary>
public class SponsorCreateDto
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(200, ErrorMessage = "Name can be at most 200 characters")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    // Optional: if null, sponsor applies to all sports
    public SportType? SportType { get; set; }

    [Required]
    public SponsorPlacement Placement { get; set; } = SponsorPlacement.Banner;

    [Url]
    [MaxLength(1000)]
    public string? PhotoUrl { get; set; }

    [Url]
    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    [Url]
    [MaxLength(1000)]
    public string? WebsiteUrl { get; set; }

    public bool IsActive { get; set; } = true;
}

