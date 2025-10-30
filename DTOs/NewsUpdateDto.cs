using System.ComponentModel.DataAnnotations;
using Kadikoy.Models;

namespace Kadikoy.DTOs;

/// <summary>
/// Haber güncelleme DTO
/// </summary>
public class NewsUpdateDto
{
    /// <summary>
    /// Haber başlığı
    /// </summary>
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string? Title { get; set; }
    
    /// <summary>
    /// Haber içeriği
    /// </summary>
    public string? Content { get; set; }
    
    /// <summary>
    /// Spor dalı (0=Hepsi, 1=Okçuluk, 2=Basketbol, 3=Voleybol)
    /// </summary>
    [Range(0, 3, ErrorMessage = "Geçerli bir spor dalı seçiniz (0-3)")]
    public SportType? SportType { get; set; }
    
    /// <summary>
    /// Haber tipi (0=Bilgilendirme, 1=SkorTakibi, 2=OzelGun)
    /// </summary>
    [Range(0, 2, ErrorMessage = "Geçerli bir haber tipi seçiniz (0-2)")]
    public NewsType? NewsType { get; set; }

    /// <summary>
    /// Yayınlanma tarihi
    /// </summary>
    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool? IsActive { get; set; }
}

