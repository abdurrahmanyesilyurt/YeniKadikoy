using Microsoft.AspNetCore.Http;

namespace Kadikoy.DTOs;

public class SponsorMediaFormDto
{
    public IFormFile File { get; set; } = null!;
}

