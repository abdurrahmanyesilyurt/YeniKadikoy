using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kadikoy.Data;
using Kadikoy.DTOs;
using Kadikoy.Interfaces;
using Kadikoy.Models;

namespace Kadikoy.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,PhotoUploader")]
public class GalleryController : ControllerBase
{
    private readonly IS3Service _s3Service;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GalleryController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly string _galleryFolder;

    public GalleryController(
        IS3Service s3Service,
        IConfiguration configuration,
        ILogger<GalleryController> logger,
        ApplicationDbContext context)
    {
        _s3Service = s3Service;
        _configuration = configuration;
        _logger = logger;
        _context = context;
        _galleryFolder = _configuration["AWS:S3:GalleryFolder"] ?? "galeri/";
    }

    /// <summary>
    /// Upload a photo to gallery
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <param name="name">Custom name for the photo (optional)</param>
    /// <returns>Upload result with file URL</returns>
    [HttpPost("upload")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<IActionResult> UploadPhoto(IFormFile file, [FromForm] string? name)
    {
        try
        {
            if (file == null)
            {
                return BadRequest(new { message = "Dosya seçilmedi" });
            }

            var username = User.Identity?.Name;
            _logger.LogInformation("User {Username} is uploading a file: {FileName}", username, file.FileName);

            var result = await _s3Service.UploadFileAsync(file, _galleryFolder);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            // Veritabanına kaydet
            var maxOrder = await _context.Galleries.MaxAsync(g => (int?)g.Order) ?? 0;
            var gallery = new Gallery
            {
                Name = name,
                FileName = result.FileName ?? string.Empty,
                S3Url = result.FileUrl ?? string.Empty,
                Order = maxOrder + 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Galleries.Add(gallery);
            await _context.SaveChangesAsync();

            _logger.LogInformation("File uploaded successfully by {Username}: {FileName} (GalleryId: {GalleryId})", username, result.FileName, gallery.Id);

            return Ok(new
            {
                message = result.Message,
                data = new
                {
                    id = gallery.Id,
                    name = gallery.Name,
                    fileName = result.FileName,
                    fileUrl = result.FileUrl,
                    order = gallery.Order,
                    fileSizeBytes = result.FileSizeBytes,
                    contentType = result.ContentType,
                    uploadedAt = result.UploadedAt,
                    uploadedBy = username
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UploadPhoto endpoint");
            return StatusCode(500, new { message = "Dosya yüklenirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Delete a photo from gallery by ID
    /// </summary>
    /// <param name="id">Gallery item ID</param>
    /// <returns>Delete result</returns>
    [HttpDelete("item/{id}")]
    public async Task<IActionResult> DeleteGalleryItem(int id)
    {
        try
        {
            var gallery = await _context.Galleries.FindAsync(id);
            if (gallery == null)
            {
                return NotFound(new { message = "Galeri öğesi bulunamadı" });
            }

            var username = User.Identity?.Name;
            _logger.LogInformation("User {Username} is deleting gallery item: {Id}", username, id);

            // S3'ten sil
            var result = await _s3Service.DeleteFileAsync(gallery.FileName, _galleryFolder);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            // Veritabanından sil
            _context.Galleries.Remove(gallery);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Gallery item deleted successfully by {Username}: {Id}", username, id);

            return Ok(new
            {
                message = "Galeri öğesi başarıyla silindi",
                data = new
                {
                    id = gallery.Id,
                    fileName = gallery.FileName,
                    deletedAt = DateTime.UtcNow,
                    deletedBy = username
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeleteGalleryItem endpoint");
            return StatusCode(500, new { message = "Galeri öğesi silinirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Delete a photo from gallery by filename (legacy)
    /// </summary>
    /// <param name="fileName">Name of the file to delete</param>
    /// <returns>Delete result</returns>
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeletePhoto(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "Dosya adı geçersiz" });
            }

            var username = User.Identity?.Name;
            _logger.LogInformation("User {Username} is deleting file: {FileName}", username, fileName);

            var result = await _s3Service.DeleteFileAsync(fileName, _galleryFolder);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            // Veritabanından da sil
            var gallery = await _context.Galleries.FirstOrDefaultAsync(g => g.FileName == fileName);
            if (gallery != null)
            {
                _context.Galleries.Remove(gallery);
                await _context.SaveChangesAsync();
            }

            _logger.LogInformation("File deleted successfully by {Username}: {FileName}", username, fileName);

            return Ok(new
            {
                message = result.Message,
                data = new
                {
                    fileName = result.FileName,
                    deletedAt = result.DeletedAt,
                    deletedBy = username
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DeletePhoto endpoint");
            return StatusCode(500, new { message = "Dosya silinirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// List all photos in gallery
    /// </summary>
    /// <returns>List of photo URLs with id and order</returns>
    [HttpGet("list")]
    [AllowAnonymous] // Public endpoint to view gallery
    public async Task<IActionResult> ListPhotos()
    {
        try
        {
            var galleries = await _context.Galleries
                .Where(g => g.IsActive)
                .OrderBy(g => g.Order)
                .Select(g => new GalleryResponseDto
                {
                    Id = g.Id,
                    Name = g.Name,
                    FileName = g.FileName,
                    FileUrl = g.S3Url,
                    Order = g.Order
                })
                .ToListAsync();

            return Ok(new
            {
                message = "Galeri başarıyla listelendi",
                count = galleries.Count,
                data = galleries
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ListPhotos endpoint");
            return StatusCode(500, new { message = "Galeri listelenirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Update order of gallery items
    /// </summary>
    /// <param name="updates">List of id and new order values</param>
    /// <returns>Update result</returns>
    [HttpPut("reorder")]
    public async Task<IActionResult> UpdateOrder([FromBody] List<GalleryOrderUpdateDto> updates)
    {
        try
        {
            if (updates == null || updates.Count == 0)
            {
                return BadRequest(new { message = "Güncellenecek öğe yok" });
            }

            var username = User.Identity?.Name;
            _logger.LogInformation("User {Username} is updating gallery order for {Count} items", username, updates.Count);

            foreach (var update in updates)
            {
                var gallery = await _context.Galleries.FindAsync(update.Id);
                if (gallery != null)
                {
                    gallery.Order = update.Order;
                    if (!string.IsNullOrWhiteSpace(update.Name))
                    {
                        gallery.Name = update.Name;
                    }
                    gallery.UpdatedAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Gallery order updated successfully by {Username}", username);

            return Ok(new
            {
                message = "Galeri sıralaması başarıyla güncellendi",
                updatedCount = updates.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UpdateOrder endpoint");
            return StatusCode(500, new { message = "Sıralama güncellenirken bir hata oluştu" });
        }
    }

    /// <summary>
    /// Get photo URL by filename
    /// </summary>
    /// <param name="fileName">Name of the file</param>
    /// <returns>Photo URL</returns>
    [HttpGet("{fileName}")]
    [AllowAnonymous]
    public IActionResult GetPhotoUrl(string fileName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return BadRequest(new { message = "Dosya adı geçersiz" });
            }

            var fileUrl = _s3Service.GetFileUrl(fileName, _galleryFolder);

            return Ok(new
            {
                fileName = fileName,
                fileUrl = fileUrl
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetPhotoUrl endpoint");
            return StatusCode(500, new { message = "URL alınırken bir hata oluştu" });
        }
    }
}

