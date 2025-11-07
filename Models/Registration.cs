using System.ComponentModel.DataAnnotations;

namespace Kadikoy.Models;

/// <summary>
/// Sporcu kayıt formu modeli
/// </summary>
public class Registration
{
    [Key]
    public int Id { get; set; }

    // Sporcu Bilgileri
    /// <summary>
    /// Sporcunun adı soyadı
    /// </summary>
    [Required(ErrorMessage = "Ad Soyadı zorunludur")]
    [MaxLength(200, ErrorMessage = "Ad Soyadı en fazla 200 karakter olabilir")]
    public string AthleteFullName { get; set; } = string.Empty;

    /// <summary>
    /// Sporcunun doğum tarihi
    /// </summary>
    [Required(ErrorMessage = "Doğum Tarihi zorunludur")]
    public DateTime BirthDate { get; set; }

    // Veli Bilgileri
    /// <summary>
    /// Velinin adı soyadı
    /// </summary>
    [Required(ErrorMessage = "Veli Ad Soyadı zorunludur")]
    [MaxLength(200, ErrorMessage = "Veli Ad Soyadı en fazla 200 karakter olabilir")]
    public string ParentFullName { get; set; } = string.Empty;

    /// <summary>
    /// Velinin telefon numarası
    /// </summary>
    [Required(ErrorMessage = "Telefon zorunludur")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
    [MaxLength(20)]
    public string ParentPhone { get; set; } = string.Empty;

    /// <summary>
    /// Velinin e-posta adresi
    /// </summary>
    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [MaxLength(255)]
    public string ParentEmail { get; set; } = string.Empty;

    /// <summary>
    /// Velinin adresi
    /// </summary>
    [Required(ErrorMessage = "Adres zorunludur")]
    [MaxLength(500, ErrorMessage = "Adres en fazla 500 karakter olabilir")]
    public string ParentAddress { get; set; } = string.Empty;

    // Program Seçimi
    /// <summary>
    /// Branş (0=Hepsi, 1=Okçuluk, 2=Basketbol, 3=Voleybol)
    /// </summary>
    [Required(ErrorMessage = "Branş zorunludur")]
    [Range(0, 3, ErrorMessage = "Geçerli bir branş seçiniz")]
    public int Branch { get; set; }

    /// <summary>
    /// Yaş Grubu (0=Hepsi, 1=Okçuluk, 2=Basketbol, 3=Voleybol)
    /// </summary>
    [Required(ErrorMessage = "Yaş Grubu zorunludur")]
    [Range(0, 3, ErrorMessage = "Geçerli bir yaş grubu seçiniz")]
    public int AgeGroup { get; set; }

    /// <summary>
    /// Program Türü (0=Hepsi, 1=Okçuluk, 2=Basketbol, 3=Voleybol)
    /// </summary>
    [Required(ErrorMessage = "Program Türü zorunludur")]
    [Range(0, 3, ErrorMessage = "Geçerli bir program türü seçiniz")]
    public int ProgramType { get; set; }

    // Sağlık Bilgileri
    /// <summary>
    /// Sağlık durumu / Özel notlar
    /// </summary>
    [MaxLength(1000, ErrorMessage = "Sağlık bilgileri en fazla 1000 karakter olabilir")]
    public string? HealthNotes { get; set; }

    // Sistem Bilgileri
    /// <summary>
    /// Kayıt tarihi
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Son güncelleme tarihi
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// Kayıt aktif mi?
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Kayıt onaylandı mı?
    /// </summary>
    public bool IsApproved { get; set; } = false;

    /// <summary>
    /// Onay tarihi
    /// </summary>
    public DateTime? ApprovedAt { get; set; }

    /// <summary>
    /// Onaylayan admin kullanıcı ID
    /// </summary>
    public int? ApprovedByUserId { get; set; }
}

