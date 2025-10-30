using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kadikoy.Data;
using Kadikoy.DTOs;
using Kadikoy.Models;
using Kadikoy.Interfaces;

namespace Kadikoy.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IS3Service _s3Service;
    private readonly ILogger<NewsController> _logger;

    public NewsController(
        ApplicationDbContext context,
        IS3Service s3Service,
        ILogger<NewsController> logger)
    {
        _context = context;
        _s3Service = s3Service;
        _logger = logger;
    }

    /// <summary>
    /// Tüm haberleri listele (filtreleme ile)
    /// </summary>
    /// <param name="sportType">Spor dalı filtresi (opsiyonel)</param>
    /// <param name="newsType">Haber tipi filtresi (opsiyonel)</param>
    /// <param name="isActive">Aktif/pasif filtresi (opsiyonel)</param>
    /// <param name="pageNumber">Sayfa numarası (varsayılan: 1)</param>
    /// <param name="pageSize">Sayfa boyutu (varsayılan: 10)</param>
    [HttpGet]
    public async Task<ActionResult<object>> GetNews(
        [FromQuery] SportType? sportType = null,
        [FromQuery] NewsType? newsType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            var query = _context.News
                .Include(n => n.MediaFiles.OrderBy(m => m.Order))
                .AsQueryable();

            // Filtreler
            if (sportType.HasValue)
            {
                query = query.Where(n => n.SportType == sportType.Value);
            }

            if (newsType.HasValue)
            {
                query = query.Where(n => n.NewsType == newsType.Value);
            }

            if (isActive.HasValue)
            {
                query = query.Where(n => n.IsActive == isActive.Value);
            }

            // Sıralama (en yeni önce)
            query = query.OrderByDescending(n => n.PublishedAt);

            // Toplam sayı
            var totalCount = await query.CountAsync();

            // Sayfalama
            var news = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var response = news.Select(n => MapToResponseDto(n)).ToList();

            return Ok(new
            {
                totalCount,
                pageNumber,
                pageSize,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                data = response
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting news list");
            return StatusCode(500, new { message = "Haberler getirilirken hata oluştu" });
        }
    }

    /// <summary>
    /// Tek haber detayı
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<NewsResponseDto>> GetNewsById(int id)
    {
        try
        {
            var news = await _context.News
                .Include(n => n.MediaFiles.OrderBy(m => m.Order))
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound(new { message = "Haber bulunamadı" });
            }

            return Ok(MapToResponseDto(news));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting news by id: {Id}", id);
            return StatusCode(500, new { message = "Haber getirilirken hata oluştu" });
        }
    }

    /// <summary>
    /// Yeni haber oluştur
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NewsResponseDto>> CreateNews([FromBody] NewsCreateDto dto)
    {
        try
        {
            var news = new News
            {
                Title = dto.Title,
                Content = dto.Content,
                SportType = dto.SportType,
                NewsType = dto.NewsType,
                PublishedAt = dto.PublishedAt ?? DateTime.UtcNow,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.News.Add(news);
            await _context.SaveChangesAsync();

            _logger.LogInformation("News created: {Id} - {Title}", news.Id, news.Title);

            return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, MapToResponseDto(news));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating news");
            return StatusCode(500, new { message = "Haber oluşturulurken hata oluştu" });
        }
    }

    /// <summary>
    /// Haber güncelle
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<NewsResponseDto>> UpdateNews(int id, [FromBody] NewsUpdateDto dto)
    {
        try
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound(new { message = "Haber bulunamadı" });
            }

            // Sadece gönderilen alanları güncelle
            if (!string.IsNullOrEmpty(dto.Title))
                news.Title = dto.Title;

            if (!string.IsNullOrEmpty(dto.Content))
                news.Content = dto.Content;

            if (dto.SportType.HasValue)
                news.SportType = dto.SportType.Value;

            if (dto.NewsType.HasValue)
                news.NewsType = dto.NewsType.Value;

            if (dto.PublishedAt.HasValue)
                news.PublishedAt = dto.PublishedAt.Value;

            if (dto.IsActive.HasValue)
                news.IsActive = dto.IsActive.Value;

            news.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("News updated: {Id} - {Title}", news.Id, news.Title);

            // Medya dosyalarını da yükle
            var updatedNews = await _context.News
                .Include(n => n.MediaFiles.OrderBy(m => m.Order))
                .FirstOrDefaultAsync(n => n.Id == id);

            return Ok(MapToResponseDto(updatedNews!));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating news: {Id}", id);
            return StatusCode(500, new { message = "Haber güncellenirken hata oluştu" });
        }
    }

    /// <summary>
    /// Haber sil
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteNews(int id)
    {
        try
        {
            var news = await _context.News
                .Include(n => n.MediaFiles)
                .FirstOrDefaultAsync(n => n.Id == id);

            if (news == null)
            {
                return NotFound(new { message = "Haber bulunamadı" });
            }

            // S3'ten medya dosyalarını sil
            foreach (var media in news.MediaFiles)
            {
                await _s3Service.DeleteNewsMediaAsync(media.S3Key);
            }

            _context.News.Remove(news);
            await _context.SaveChangesAsync();

            _logger.LogInformation("News deleted: {Id} - {Title}", news.Id, news.Title);

            return Ok(new { message = "Haber başarıyla silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting news: {Id}", id);
            return StatusCode(500, new { message = "Haber silinirken hata oluştu" });
        }
    }

    /// <summary>
    /// Habere medya ekle (fotoğraf veya video)
    /// </summary>
    [HttpPost("{id}/media")]
    [Consumes("multipart/form-data")]
    [Authorize(Roles = "Admin,PhotoUploader")]
    [RequestSizeLimit(100 * 1024 * 1024)] // 100 MB limit (videolar için)
    public async Task<ActionResult<NewsMediaUploadResponseDto>> UploadNewsMedia(
        int id,
        [FromForm] NewsMediaFormDto form)
    {
        try
        {
            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound(new { message = "Haber bulunamadı" });
            }

            var mediaTypeEnum = (MediaType)form.MediaType;

            // S3'e yükle
            var uploadResult = await _s3Service.UploadNewsMediaAsync(form.File, news.SportType, mediaTypeEnum);

            if (!uploadResult.Success)
            {
                return BadRequest(new NewsMediaUploadResponseDto
                {
                    Success = false,
                    Message = uploadResult.Message
                });
            }

            // Veritabanına kaydet
            var newsMedia = new NewsMedia
            {
                NewsId = id,
                MediaType = mediaTypeEnum,
                S3Key = uploadResult.S3Key!,
                S3Url = uploadResult.FileUrl!,
                FileName = form.File.FileName,
                FileSize = form.File.Length,
                Order = form.Order,
                UploadedAt = DateTime.UtcNow
            };

            _context.NewsMedia.Add(newsMedia);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Media uploaded for news {NewsId}: {MediaId}", id, newsMedia.Id);

            return Ok(new NewsMediaUploadResponseDto
            {
                Success = true,
                Message = "Medya başarıyla yüklendi",
                Media = new NewsMediaResponseDto
                {
                    Id = newsMedia.Id,
                    MediaType = newsMedia.MediaType,
                    MediaTypeName = newsMedia.MediaType.ToString(),
                    S3Url = newsMedia.S3Url,
                    FileName = newsMedia.FileName,
                    FileSize = newsMedia.FileSize,
                    Order = newsMedia.Order,
                    UploadedAt = newsMedia.UploadedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading media for news: {Id}", id);
            return StatusCode(500, new NewsMediaUploadResponseDto
            {
                Success = false,
                Message = "Medya yüklenirken hata oluştu"
            });
        }
    }

    /// <summary>
    /// Medya sil
    /// </summary>
    [HttpDelete("media/{mediaId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteNewsMedia(int mediaId)
    {
        try
        {
            var media = await _context.NewsMedia.FindAsync(mediaId);
            if (media == null)
            {
                return NotFound(new { message = "Medya bulunamadı" });
            }

            // S3'ten sil
            var deleteResult = await _s3Service.DeleteNewsMediaAsync(media.S3Key);

            if (!deleteResult.Success)
            {
                _logger.LogWarning("Failed to delete media from S3: {S3Key}", media.S3Key);
            }

            // Veritabanından sil
            _context.NewsMedia.Remove(media);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Media deleted: {MediaId}", mediaId);

            return Ok(new { message = "Medya başarıyla silindi" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting media: {MediaId}", mediaId);
            return StatusCode(500, new { message = "Medya silinirken hata oluştu" });
        }
    }
    /// <summary>
    /// Fotoğraf sayıları (toplam ve spor dalına göre)
    /// </summary>
    [HttpGet("media/photo-stats")]
    [Authorize(Roles = "Admin,PhotoUploader")]
    public async Task<ActionResult<PhotoStatsResponseDto>> GetPhotoStats()
    {
        try
        {
            var query = _context.NewsMedia
                .Include(nm => nm.News)
                .Where(nm => nm.MediaType == MediaType.Photo);

            var grouped = await query
                .GroupBy(nm => nm.News!.SportType)
                .Select(g => new { SportType = g.Key, Count = g.Count() })
                .ToListAsync();

            int okculuk = grouped.FirstOrDefault(x => x.SportType == SportType.Okculuk)?.Count ?? 0;
            int basketbol = grouped.FirstOrDefault(x => x.SportType == SportType.Basketbol)?.Count ?? 0;
            int voleybol = grouped.FirstOrDefault(x => x.SportType == SportType.Voleybol)?.Count ?? 0;
            int total = await query.CountAsync();

            return Ok(new PhotoStatsResponseDto
            {
                Total = total,
                Okculuk = okculuk,
                Basketbol = basketbol,
                Voleybol = voleybol
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting photo stats");
            return StatusCode(500, new { message = "Fotoğraf istatistikleri getirilirken hata oluştu" });
        }
    }


    // Helper method
    private NewsResponseDto MapToResponseDto(News news)
    {
        return new NewsResponseDto
        {
            Id = news.Id,
            Title = news.Title,
            Content = news.Content,
            SportType = news.SportType,
            SportTypeName = news.SportType.ToString(),
            NewsType = news.NewsType,
            NewsTypeName = news.NewsType.ToString(),
            PublishedAt = news.PublishedAt,
            CreatedAt = news.CreatedAt,
            UpdatedAt = news.UpdatedAt,
            IsActive = news.IsActive,
            MediaFiles = news.MediaFiles?.Select(m => new NewsMediaResponseDto
            {
                Id = m.Id,
                MediaType = m.MediaType,
                MediaTypeName = m.MediaType.ToString(),
                S3Url = m.S3Url,
                FileName = m.FileName,
                FileSize = m.FileSize,
                Order = m.Order,
                UploadedAt = m.UploadedAt
            }).ToList() ?? new List<NewsMediaResponseDto>()
        };
    }
}

