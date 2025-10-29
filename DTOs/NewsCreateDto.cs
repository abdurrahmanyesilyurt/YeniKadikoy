using System.ComponentModel.DataAnnotations;
using Kadikoy.Models;

namespace Kadikoy.DTOs;

/// <summary>
/// Yeni haber oluşturma DTO
/// </summary>
public class NewsCreateDto
{
    /// <summary>
    /// Haber başlığı
    /// </summary>
    [Required(ErrorMessage = "Başlık zorunludur")]
    [MaxLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir")]
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Haber içeriği
    /// </summary>
    [Required(ErrorMessage = "İçerik zorunludur")]
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Spor dalı (0=Hepsi, 1=Okçuluk, 2=Basketbol, 3=Voleybol)
    /// </summary>
    [Required(ErrorMessage = "Spor dalı zorunludur")]
    [Range(0, 3, ErrorMessage = "Geçerli bir spor dalı seçiniz (0-3)")]
    public SportType SportType { get; set; }
    
    /// <summary>
    /// Yayınlanma tarihi (opsiyonel, belirtilmezse şu an)
    /// </summary>
    public DateTime? PublishedAt { get; set; }
    
    /// <summary>
    /// Aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;
}

