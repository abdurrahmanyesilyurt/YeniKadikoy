using System.ComponentModel.DataAnnotations;

namespace Kadikoy.Models;

/// <summary>
/// Galeri fotoğrafı modeli
/// </summary>
public class Gallery
{
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// Fotoğrafın özel adı (kullanıcı tarafından girilir)
    /// </summary>
    [MaxLength(255)]
    public string? Name { get; set; }

    /// <summary>
    /// S3'teki dosya adı
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// S3'teki dosyanın tam URL'i
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string S3Url { get; set; } = string.Empty;

    /// <summary>
    /// Sıralama (küçük sayı önce gösterilir)
    /// </summary>
    public int Order { get; set; } = 0;

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Güncellenme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

