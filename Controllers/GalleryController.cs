using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kadikoy.Interfaces;

namespace Kadikoy.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,PhotoUploader")]
public class GalleryController : ControllerBase
{
    private readonly IS3Service _s3Service;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GalleryController> _logger;
    private readonly string _galleryFolder;

    public GalleryController(
        IS3Service s3Service, 
        IConfiguration configuration, 
        ILogger<GalleryController> logger)
    {
        _s3Service = s3Service;
        _configuration = configuration;
        _logger = logger;
        _galleryFolder = _configuration["AWS:S3:GalleryFolder"] ?? "galeri/";
    }

    /// <summary>
    /// Upload a photo to gallery
    /// </summary>
    /// <param name="file">Image file to upload</param>
    /// <returns>Upload result with file URL</returns>
    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
    public async Task<IActionResult> UploadPhoto([FromForm] IFormFile file)
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

            _logger.LogInformation("File uploaded successfully by {Username}: {FileName}", username, result.FileName);

            return Ok(new
            {
                message = result.Message,
                data = new
                {
                    fileName = result.FileName,
                    fileUrl = result.FileUrl,
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
    /// Delete a photo from gallery
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
    /// <returns>List of photo URLs</returns>
    [HttpGet("list")]
    [AllowAnonymous] // Public endpoint to view gallery
    public async Task<IActionResult> ListPhotos()
    {
        try
        {
            var files = await _s3Service.ListFilesAsync(_galleryFolder);

            var fileUrls = files.Select(fileName => new
            {
                fileName = fileName,
                fileUrl = _s3Service.GetFileUrl(fileName, _galleryFolder)
            }).ToList();

            return Ok(new
            {
                message = "Galeri başarıyla listelendi",
                count = fileUrls.Count,
                data = fileUrls
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ListPhotos endpoint");
            return StatusCode(500, new { message = "Galeri listelenirken bir hata oluştu" });
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

