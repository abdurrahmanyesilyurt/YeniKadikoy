using System.ComponentModel.DataAnnotations;
using Kadikoy.Models;

namespace Kadikoy.DTOs;

/// <summary>
/// Haber medya yükleme DTO
/// </summary>
public class NewsMediaUploadDto
{
    /// <summary>
    /// Haber ID
    /// </summary>
    [Required(ErrorMessage = "Haber ID zorunludur")]
    public int NewsId { get; set; }
    
    /// <summary>
    /// Medya türü (0=Fotoğraf, 1=Video)
    /// </summary>
    [Required(ErrorMessage = "Medya türü zorunludur")]
    [Range(0, 1, ErrorMessage = "Geçerli bir medya türü seçiniz (0=Fotoğraf, 1=Video)")]
    public MediaType MediaType { get; set; }
    
    /// <summary>
    /// Sıralama
    /// </summary>
    public int Order { get; set; } = 0;
}

/// <summary>
/// Medya yükleme response DTO
/// </summary>
public class NewsMediaUploadResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public NewsMediaResponseDto? Media { get; set; }
}

