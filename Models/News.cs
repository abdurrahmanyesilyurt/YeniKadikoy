using System.ComponentModel.DataAnnotations;

namespace Kadikoy.Models;

/// <summary>
/// Haber modeli
/// </summary>
public class News
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Haber başlığı
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Haber içeriği
    /// </summary>
    [Required]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Hangi spor dalına ait (0=Hepsi, 1=Okçuluk, 2=Basketbol, 3=Voleybol)
    /// </summary>
    [Required]
    public SportType SportType { get; set; }

    /// <summary>
    /// Haber tipi (0=Bilgilendirme, 1=SkorTakibi, 2=OzelGun)
    /// </summary>
    [Required]
    public NewsType NewsType { get; set; } = NewsType.Bilgilendirme;

    /// <summary>
    /// Haberin yayınlanma tarihi
    /// </summary>
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Oluşturulma tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Güncellenme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Haberin aktif olup olmadığı
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Habere ait medya dosyaları (fotoğraf ve videolar)
    /// </summary>
    public virtual ICollection<NewsMedia> MediaFiles { get; set; } = new List<NewsMedia>();
}

