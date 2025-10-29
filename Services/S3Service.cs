using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Kadikoy.DTOs;
using Kadikoy.Interfaces;

namespace Kadikoy.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<S3Service> _logger;
    private readonly string _bucketName;
    private readonly long _maxFileSizeBytes;
    private readonly string[] _allowedExtensions;

    public S3Service(IAmazonS3 s3Client, IConfiguration configuration, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _logger = logger;
        _bucketName = _configuration["AWS:S3:BucketName"] ?? throw new InvalidOperationException("S3 Bucket name not configured");
        
        var maxFileSizeMB = int.Parse(_configuration["AWS:S3:MaxFileSizeMB"] ?? "10");
        _maxFileSizeBytes = maxFileSizeMB * 1024 * 1024;
        
        _allowedExtensions = _configuration.GetSection("AWS:S3:AllowedExtensions").Get<string[]>() 
            ?? new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
    }

    public async Task<MediaUploadResponseDto> UploadFileAsync(IFormFile file, string folder)
    {
        try
        {
            // Validate file
            if (file == null || file.Length == 0)
            {
                return new MediaUploadResponseDto
                {
                    Success = false,
                    Message = "Dosya boş veya geçersiz"
                };
            }

            // Check file size
            if (file.Length > _maxFileSizeBytes)
            {
                return new MediaUploadResponseDto
                {
                    Success = false,
                    Message = $"Dosya boyutu {_maxFileSizeBytes / (1024 * 1024)} MB'dan büyük olamaz"
                };
            }

            // Check file extension
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(fileExtension))
            {
                return new MediaUploadResponseDto
                {
                    Success = false,
                    Message = $"Sadece şu dosya formatları kabul edilir: {string.Join(", ", _allowedExtensions)}"
                };
            }

            // Generate unique file name
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var key = $"{folder}{uniqueFileName}";

            // Upload to S3
            using var stream = file.OpenReadStream();
            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = _bucketName,
                ContentType = file.ContentType,
                CannedACL = S3CannedACL.PublicRead // Make file publicly accessible
            };

            var transferUtility = new TransferUtility(_s3Client);
            await transferUtility.UploadAsync(uploadRequest);

            var fileUrl = GetFileUrl(uniqueFileName, folder);

            _logger.LogInformation("File uploaded successfully: {FileName} to {Key}", file.FileName, key);

            return new MediaUploadResponseDto
            {
                Success = true,
                Message = "Dosya başarıyla yüklendi",
                FileName = uniqueFileName,
                FileUrl = fileUrl,
                FileSizeBytes = file.Length,
                ContentType = file.ContentType,
                UploadedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 error while uploading file: {FileName}", file.FileName);
            return new MediaUploadResponseDto
            {
                Success = false,
                Message = $"S3 hatası: {ex.Message}"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file: {FileName}", file.FileName);
            return new MediaUploadResponseDto
            {
                Success = false,
                Message = $"Dosya yüklenirken hata oluştu: {ex.Message}"
            };
        }
    }

    public async Task<MediaDeleteResponseDto> DeleteFileAsync(string fileName, string folder)
    {
        try
        {
            var key = $"{folder}{fileName}";

            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest);

            _logger.LogInformation("File deleted successfully: {Key}", key);

            return new MediaDeleteResponseDto
            {
                Success = true,
                Message = "Dosya başarıyla silindi",
                FileName = fileName,
                DeletedAt = DateTime.UtcNow
            };
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "AWS S3 error while deleting file: {FileName}", fileName);
            return new MediaDeleteResponseDto
            {
                Success = false,
                Message = $"S3 hatası: {ex.Message}",
                FileName = fileName,
                DeletedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file: {FileName}", fileName);
            return new MediaDeleteResponseDto
            {
                Success = false,
                Message = $"Dosya silinirken hata oluştu: {ex.Message}",
                FileName = fileName,
                DeletedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<List<string>> ListFilesAsync(string folder)
    {
        try
        {
            var request = new ListObjectsV2Request
            {
                BucketName = _bucketName,
                Prefix = folder
            };

            var response = await _s3Client.ListObjectsV2Async(request);
            
            return response.S3Objects
                .Select(obj => obj.Key.Replace(folder, ""))
                .Where(key => !string.IsNullOrEmpty(key))
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error listing files in folder: {Folder}", folder);
            return new List<string>();
        }
    }

    public string GetFileUrl(string fileName, string folder)
    {
        var region = _configuration["AWS:Region"] ?? "eu-north-1";
        return $"https://{_bucketName}.s3.{region}.amazonaws.com/{folder}{fileName}";
    }
}

