namespace Kadikoy.DTOs;

/// <summary>
/// Sporcu kayıt formu response DTO
/// </summary>
public class RegistrationResponseDto
{
    public int Id { get; set; }

    // Sporcu Bilgileri
    public string AthleteFullName { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; }

    // Veli Bilgileri
    public string ParentFullName { get; set; } = string.Empty;
    public string ParentPhone { get; set; } = string.Empty;
    public string ParentEmail { get; set; } = string.Empty;
    public string ParentAddress { get; set; } = string.Empty;

    // Program Seçimi
    public int Branch { get; set; }
    public int AgeGroup { get; set; }
    public int ProgramType { get; set; }

    // Sağlık Bilgileri
    public string? HealthNotes { get; set; }

    // Sistem Bilgileri
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public bool IsApproved { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public int? ApprovedByUserId { get; set; }
}

