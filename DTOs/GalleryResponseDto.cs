namespace Kadikoy.DTOs;

/// <summary>
/// Galeri fotoğrafı response DTO
/// </summary>
public class GalleryResponseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FileUrl { get; set; } = string.Empty;
    public int Order { get; set; }
}

