using System.ComponentModel.DataAnnotations;
using Kadikoy.Models;

namespace Kadikoy.DTOs;

/// <summary>
/// Update DTO for Sponsor
/// </summary>
public class SponsorUpdateDto
{
    [MaxLength(200)]
    public string? Name { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    public SportType? SportType { get; set; }

    public SponsorPlacement? Placement { get; set; }

    [Url]
    [MaxLength(1000)]
    public string? PhotoUrl { get; set; }

    [Url]
    [MaxLength(1000)]
    public string? LogoUrl { get; set; }

    [Url]
    [MaxLength(1000)]
    public string? WebsiteUrl { get; set; }

    public bool? IsActive { get; set; }
}

