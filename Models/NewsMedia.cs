using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kadikoy.Models;

/// <summary>
/// Haber medya dosyaları (fotoğraf ve video)
/// </summary>
public class NewsMedia
{
    [Key]
    public int Id { get; set; }
    
    /// <summary>
    /// Hangi habere ait
    /// </summary>
    [Required]
    public int NewsId { get; set; }
    
    /// <summary>
    /// Medya türü (0=Fotoğraf, 1=Video)
    /// </summary>
    [Required]
    public MediaType MediaType { get; set; }
    
    /// <summary>
    /// S3'teki dosya anahtarı (path)
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string S3Key { get; set; } = string.Empty;
    
    /// <summary>
    /// S3'teki dosyanın tam URL'i
    /// </summary>
    [Required]
    [MaxLength(1000)]
    public string S3Url { get; set; } = string.Empty;
    
    /// <summary>
    /// Dosya adı
    /// </summary>
    [MaxLength(255)]
    public string? FileName { get; set; }
    
    /// <summary>
    /// Dosya boyutu (bytes)
    /// </summary>
    public long FileSize { get; set; }
    
    /// <summary>
    /// Medya sıralaması (gösterim sırası için)
    /// </summary>
    public int Order { get; set; } = 0;
    
    /// <summary>
    /// Yüklenme tarihi
    /// </summary>
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// İlişkili haber
    /// </summary>
    [ForeignKey("NewsId")]
    public virtual News? News { get; set; }
}

