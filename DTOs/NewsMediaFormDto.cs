using Microsoft.AspNetCore.Http;

namespace Kadikoy.DTOs;

public class NewsMediaFormDto
{
    public IFormFile File { get; set; } = null!;
    public int MediaType { get; set; } = 0; // 0=Photo, 1=Video
    public int Order { get; set; } = 0;
}

