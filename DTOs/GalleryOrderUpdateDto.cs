namespace Kadikoy.DTOs;

/// <summary>
/// Galeri öğesinin sırasını güncellemek için DTO
/// </summary>
public class GalleryOrderUpdateDto
{
    public int Id { get; set; }
    public int Order { get; set; }
    public string? Name { get; set; }
}

