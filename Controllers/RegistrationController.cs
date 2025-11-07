using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kadikoy.Data;
using Kadikoy.DTOs;
using Kadikoy.Models;

namespace Kadikoy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegistrationController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RegistrationController> _logger;

    public RegistrationController(ApplicationDbContext context, ILogger<RegistrationController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Yeni sporcu kaydı oluştur
    /// </summary>
    /// <param name="dto">Kayıt bilgileri</param>
    /// <returns>Oluşturulan kayıt</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegistrationCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Geçersiz kayıt bilgileri", errors = ModelState });
            }

            // Aynı e-posta ile kayıt var mı kontrol et
            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.ParentEmail == dto.ParentEmail);

            if (existingRegistration != null)
            {
                return BadRequest(new { message = "Bu e-posta adresi ile zaten bir kayıt bulunmaktadır" });
            }

            var registration = new Registration
            {
                AthleteFullName = dto.AthleteFullName,
                BirthDate = dto.BirthDate,
                ParentFullName = dto.ParentFullName,
                ParentPhone = dto.ParentPhone,
                ParentEmail = dto.ParentEmail,
                ParentAddress = dto.ParentAddress,
                Branch = dto.Branch,
                AgeGroup = dto.AgeGroup,
                ProgramType = dto.ProgramType,
                HealthNotes = dto.HealthNotes,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsApproved = false
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Yeni kayıt oluşturuldu: {AthleteFullName} ({ParentEmail})", 
                dto.AthleteFullName, dto.ParentEmail);

            var response = MapToResponseDto(registration);
            return CreatedAtAction(nameof(GetRegistrationById), new { id = registration.Id }, 
                new { message = "Kayıt başarıyla oluşturuldu", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt oluşturulurken hata oluştu");
            return StatusCode(500, new { message = "Kayıt oluşturulurken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Kayıt ID'sine göre getir
    /// </summary>
    /// <param name="id">Kayıt ID'si</param>
    /// <returns>Kayıt bilgileri</returns>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRegistrationById(int id)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);

            if (registration == null)
            {
                return NotFound(new { message = "Kayıt bulunamadı" });
            }

            var response = MapToResponseDto(registration);
            return Ok(new { data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt getirilirken hata oluştu: {Id}", id);
            return StatusCode(500, new { message = "Kayıt getirilirken bir hata oluştu" });
        }
    }

    // ==================== ADMIN ENDPOINTS ====================

    /// <summary>
    /// Tüm kayıtları getir (Admin)
    /// </summary>
    /// <param name="page">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
    /// <param name="isApproved">Onaylı kayıtları filtrele (null=tümü)</param>
    /// <returns>Kayıtlar listesi</returns>
    [HttpGet("admin/all")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAllRegistrations(int page = 1, int pageSize = 10, bool? isApproved = null)
    {
        try
        {
            var query = _context.Registrations.AsQueryable();

            // Filtrele
            if (isApproved.HasValue)
            {
                query = query.Where(r => r.IsApproved == isApproved.Value);
            }

            // Toplam kayıt sayısı
            var totalCount = await query.CountAsync();

            // Sayfalama
            var registrations = await query
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = registrations.Select(MapToResponseDto).ToList();

            _logger.LogInformation("Admin tarafından {Count} kayıt listelendi", registrations.Count);

            return Ok(new
            {
                message = "Kayıtlar başarıyla getirildi",
                data = response,
                pagination = new
                {
                    page = page,
                    pageSize = pageSize,
                    totalCount = totalCount,
                    totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıtlar listelenirken hata oluştu");
            return StatusCode(500, new { message = "Kayıtlar listelenirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Kayıt istatistikleri (Admin)
    /// </summary>
    /// <returns>İstatistikler</returns>
    [HttpGet("admin/stats")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetRegistrationStats()
    {
        try
        {
            var totalCount = await _context.Registrations.CountAsync();
            var approvedCount = await _context.Registrations.CountAsync(r => r.IsApproved);
            var pendingCount = await _context.Registrations.CountAsync(r => !r.IsApproved);
            var activeCount = await _context.Registrations.CountAsync(r => r.IsActive);

            // Branş bazında istatistikler
            var branchStats = await _context.Registrations
                .GroupBy(r => r.Branch)
                .Select(g => new { branch = g.Key, count = g.Count() })
                .ToListAsync();

            // Yaş grubu bazında istatistikler
            var ageGroupStats = await _context.Registrations
                .GroupBy(r => r.AgeGroup)
                .Select(g => new { ageGroup = g.Key, count = g.Count() })
                .ToListAsync();

            _logger.LogInformation("Admin tarafından istatistikler görüntülendi");

            return Ok(new
            {
                message = "İstatistikler başarıyla getirildi",
                data = new
                {
                    totalCount = totalCount,
                    approvedCount = approvedCount,
                    pendingCount = pendingCount,
                    activeCount = activeCount,
                    branchStats = branchStats,
                    ageGroupStats = ageGroupStats
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "İstatistikler getirilirken hata oluştu");
            return StatusCode(500, new { message = "İstatistikler getirilirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Kayıt onayı (Admin)
    /// </summary>
    /// <param name="id">Kayıt ID'si</param>
    /// <returns>Onaylanan kayıt</returns>
    [HttpPut("admin/{id}/approve")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ApproveRegistration(int id)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);

            if (registration == null)
            {
                return NotFound(new { message = "Kayıt bulunamadı" });
            }

            if (registration.IsApproved)
            {
                return BadRequest(new { message = "Bu kayıt zaten onaylanmış" });
            }

            registration.IsApproved = true;
            registration.ApprovedAt = DateTime.UtcNow;
            registration.UpdatedAt = DateTime.UtcNow;

            // Admin ID'sini claim'den al
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                registration.ApprovedByUserId = userId;
            }

            _context.Registrations.Update(registration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kayıt onaylandı: {Id} ({AthleteFullName})", id, registration.AthleteFullName);

            var response = MapToResponseDto(registration);
            return Ok(new { message = "Kayıt başarıyla onaylandı", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt onaylanırken hata oluştu: {Id}", id);
            return StatusCode(500, new { message = "Kayıt onaylanırken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Kayıt sil (Admin)
    /// </summary>
    /// <param name="id">Kayıt ID'si</param>
    /// <returns>Silme sonucu</returns>
    [HttpDelete("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteRegistration(int id)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);

            if (registration == null)
            {
                return NotFound(new { message = "Kayıt bulunamadı" });
            }

            _context.Registrations.Remove(registration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kayıt silindi: {Id} ({AthleteFullName})", id, registration.AthleteFullName);

            return Ok(new { message = "Kayıt başarıyla silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt silinirken hata oluştu: {Id}", id);
            return StatusCode(500, new { message = "Kayıt silinirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Kayıt güncelle (Admin)
    /// </summary>
    /// <param name="id">Kayıt ID'si</param>
    /// <param name="dto">Güncellenecek bilgiler</param>
    /// <returns>Güncellenmiş kayıt</returns>
    [HttpPut("admin/{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRegistration(int id, [FromBody] RegistrationCreateDto dto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Geçersiz güncelleme bilgileri", errors = ModelState });
            }

            var registration = await _context.Registrations.FindAsync(id);

            if (registration == null)
            {
                return NotFound(new { message = "Kayıt bulunamadı" });
            }

            // E-posta değiştiriliyorsa, başka bir kayıtta kullanılıp kullanılmadığını kontrol et
            if (registration.ParentEmail != dto.ParentEmail)
            {
                var existingEmail = await _context.Registrations
                    .FirstOrDefaultAsync(r => r.ParentEmail == dto.ParentEmail && r.Id != id);

                if (existingEmail != null)
                {
                    return BadRequest(new { message = "Bu e-posta adresi başka bir kayıtta kullanılmaktadır" });
                }
            }

            registration.AthleteFullName = dto.AthleteFullName;
            registration.BirthDate = dto.BirthDate;
            registration.ParentFullName = dto.ParentFullName;
            registration.ParentPhone = dto.ParentPhone;
            registration.ParentEmail = dto.ParentEmail;
            registration.ParentAddress = dto.ParentAddress;
            registration.Branch = dto.Branch;
            registration.AgeGroup = dto.AgeGroup;
            registration.ProgramType = dto.ProgramType;
            registration.HealthNotes = dto.HealthNotes;
            registration.UpdatedAt = DateTime.UtcNow;

            _context.Registrations.Update(registration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kayıt güncellendi: {Id} ({AthleteFullName})", id, registration.AthleteFullName);

            var response = MapToResponseDto(registration);
            return Ok(new { message = "Kayıt başarıyla güncellendi", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt güncellenirken hata oluştu: {Id}", id);
            return StatusCode(500, new { message = "Kayıt güncellenirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Kayıt durumunu değiştir (Admin)
    /// </summary>
    /// <param name="id">Kayıt ID'si</param>
    /// <param name="request">isActive alanı içeren request</param>
    /// <returns>Güncellenmiş kayıt</returns>
    [HttpPut("admin/{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateRegistrationStatus(int id, [FromBody] dynamic request)
    {
        try
        {
            var registration = await _context.Registrations.FindAsync(id);

            if (registration == null)
            {
                return NotFound(new { message = "Kayıt bulunamadı" });
            }

            bool isActive = request.isActive;
            registration.IsActive = isActive;
            registration.UpdatedAt = DateTime.UtcNow;

            _context.Registrations.Update(registration);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Kayıt durumu güncellendi: {Id} - IsActive: {IsActive}", id, isActive);

            var response = MapToResponseDto(registration);
            return Ok(new { message = "Kayıt durumu başarıyla güncellendi", data = response });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kayıt durumu güncellenirken hata oluştu: {Id}", id);
            return StatusCode(500, new { message = "Kayıt durumu güncellenirken bir hata oluştu" });
        }
    }

    private RegistrationResponseDto MapToResponseDto(Registration registration)
    {
        return new RegistrationResponseDto
        {
            Id = registration.Id,
            AthleteFullName = registration.AthleteFullName,
            BirthDate = registration.BirthDate,
            ParentFullName = registration.ParentFullName,
            ParentPhone = registration.ParentPhone,
            ParentEmail = registration.ParentEmail,
            ParentAddress = registration.ParentAddress,
            Branch = registration.Branch,
            AgeGroup = registration.AgeGroup,
            ProgramType = registration.ProgramType,
            HealthNotes = registration.HealthNotes,
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt,
            IsActive = registration.IsActive,
            IsApproved = registration.IsApproved,
            ApprovedAt = registration.ApprovedAt,
            ApprovedByUserId = registration.ApprovedByUserId
        };
    }
}

