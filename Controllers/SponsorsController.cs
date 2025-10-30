using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Kadikoy.Data;
using Kadikoy.DTOs;
using Kadikoy.Models;
using Kadikoy.Interfaces;

namespace Kadikoy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SponsorsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IS3Service _s3Service;
    private readonly ILogger<SponsorsController> _logger;

    public SponsorsController(ApplicationDbContext context, IS3Service s3Service, ILogger<SponsorsController> logger)
    {
        _context = context;
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// List sponsors with optional filters and pagination
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<object>> GetSponsors(
        [FromQuery] SportType? sportType = null,
        [FromQuery] SponsorPlacement? placement = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.Sponsors.AsQueryable();

            if (sportType.HasValue)
                query = query.Where(s => s.SportType == sportType.Value);

            if (placement.HasValue)
                query = query.Where(s => s.Placement == placement.Value);

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            query = query.OrderByDescending(s => s.CreatedAt);

            var totalCount = await query.CountAsync();

            var sponsors = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var data = sponsors.Select(MapToResponseDto).ToList();

            return Ok(new
            {
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sponsors");
            return StatusCode(500, new { message = "Sponsorlar getirilirken hata oluştu" });
        }
    }

    /// <summary>
    /// Get sponsor by id
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SponsorResponseDto>> GetSponsorById(int id)
    {
        try
        {
            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
                return NotFound(new { message = "Sponsor bulunamadı" });

            return Ok(MapToResponseDto(sponsor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sponsor by id: {Id}", id);
            return StatusCode(500, new { message = "Sponsor getirilirken hata oluştu" });
        }
    }

    /// <summary>
    /// Create a sponsor
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SponsorResponseDto>> CreateSponsor([FromBody] SponsorCreateDto dto)
    {
        try
        {
            var sponsor = new Sponsor
            {
                Name = dto.Name,
                Description = dto.Description,
                SportType = dto.SportType,
                Placement = dto.Placement,
                PhotoUrl = dto.PhotoUrl,
                LogoUrl = dto.LogoUrl,
                WebsiteUrl = dto.WebsiteUrl,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Sponsors.Add(sponsor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsor created: {Id} - {Name}", sponsor.Id, sponsor.Name);

            return CreatedAtAction(nameof(GetSponsorById), new { id = sponsor.Id }, MapToResponseDto(sponsor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sponsor");
            return StatusCode(500, new { message = "Sponsor oluşturulurken hata oluştu" });
        }
    }

    /// <summary>
    /// Update a sponsor
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<SponsorResponseDto>> UpdateSponsor(int id, [FromBody] SponsorUpdateDto dto)
    {
        try
        {
            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
                return NotFound(new { message = "Sponsor bulunamadı" });

            if (!string.IsNullOrWhiteSpace(dto.Name)) sponsor.Name = dto.Name;
            if (!string.IsNullOrWhiteSpace(dto.Description)) sponsor.Description = dto.Description;
            if (dto.SportType.HasValue) sponsor.SportType = dto.SportType.Value;
            if (dto.Placement.HasValue) sponsor.Placement = dto.Placement.Value;
            if (!string.IsNullOrWhiteSpace(dto.PhotoUrl)) sponsor.PhotoUrl = dto.PhotoUrl;
            if (!string.IsNullOrWhiteSpace(dto.LogoUrl)) sponsor.LogoUrl = dto.LogoUrl;
            if (!string.IsNullOrWhiteSpace(dto.WebsiteUrl)) sponsor.WebsiteUrl = dto.WebsiteUrl;
            if (dto.IsActive.HasValue) sponsor.IsActive = dto.IsActive.Value;

            sponsor.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsor updated: {Id} - {Name}", sponsor.Id, sponsor.Name);

            return Ok(MapToResponseDto(sponsor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating sponsor: {Id}", id);
            return StatusCode(500, new { message = "Sponsor güncellenirken hata oluştu" });
        }
    }

    /// <summary>
    /// Delete a sponsor
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteSponsor(int id)
    {
        try
        {
            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
                return NotFound(new { message = "Sponsor bulunamadı" });

            _context.Sponsors.Remove(sponsor);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsor deleted: {Id} - {Name}", sponsor.Id, sponsor.Name);

            return Ok(new { message = "Sponsor başarıyla silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting sponsor: {Id}", id);
            return StatusCode(500, new { message = "Sponsor silinirken hata oluştu" });
        }
    }

    /// <summary>
    /// Upload sponsor photo (multipart/form-data). Also supports URL via JSON update endpoints.
    /// </summary>
    [HttpPost("{id}/photo")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Admin,PhotoUploader")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult<SponsorResponseDto>> UploadPhoto(int id, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null)
                return BadRequest(new { message = "Dosya seçilmedi" });

            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
                return NotFound(new { message = "Sponsor bulunamadı" });

            var result = await _s3Service.UploadFileAsync(file, "sponsors/photos/");
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            sponsor.PhotoUrl = result.FileUrl;
            sponsor.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsor photo uploaded: {Id} - {FileName}", id, file.FileName);

            return Ok(MapToResponseDto(sponsor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading sponsor photo: {Id}", id);
            return StatusCode(500, new { message = "Fotoğraf yüklenirken hata oluştu" });
        }
    }

    /// <summary>
    /// Upload sponsor logo (multipart/form-data). Also supports URL via JSON update endpoints.
    /// </summary>
    [HttpPost("{id}/logo")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Admin,PhotoUploader")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
    public async Task<ActionResult<SponsorResponseDto>> UploadLogo(int id, [FromForm] IFormFile file)
    {
        try
        {
            if (file == null)
                return BadRequest(new { message = "Dosya seçilmedi" });

            var sponsor = await _context.Sponsors.FindAsync(id);
            if (sponsor == null)
                return NotFound(new { message = "Sponsor bulunamadı" });

            var result = await _s3Service.UploadFileAsync(file, "sponsors/logos/");
            if (!result.Success)
                return BadRequest(new { message = result.Message });

            sponsor.LogoUrl = result.FileUrl;
            sponsor.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Sponsor logo uploaded: {Id} - {FileName}", id, file.FileName);

            return Ok(MapToResponseDto(sponsor));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading sponsor logo: {Id}", id);
            return StatusCode(500, new { message = "Logo yüklenirken hata oluştu" });
        }
    }

    private SponsorResponseDto MapToResponseDto(Sponsor s) => new SponsorResponseDto
    {
        Id = s.Id,
        Name = s.Name,
        Description = s.Description,
        SportType = s.SportType,
        SportTypeName = s.SportType?.ToString() ?? string.Empty,
        Placement = s.Placement,
        PlacementName = s.Placement.ToString(),
        PhotoUrl = s.PhotoUrl,
        LogoUrl = s.LogoUrl,
        WebsiteUrl = s.WebsiteUrl,
        IsActive = s.IsActive,
        CreatedAt = s.CreatedAt,
        UpdatedAt = s.UpdatedAt
    };
}

