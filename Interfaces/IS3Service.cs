using Kadikoy.DTOs;
using Kadikoy.Models;

namespace Kadikoy.Interfaces;

public interface IS3Service
{
    Task<MediaUploadResponseDto> UploadFileAsync(IFormFile file, string folder);
    Task<MediaDeleteResponseDto> DeleteFileAsync(string fileName, string folder);
    Task<List<string>> ListFilesAsync(string folder);
    string GetFileUrl(string fileName, string folder);

    // Haber medya metodları
    Task<MediaUploadResponseDto> UploadNewsMediaAsync(IFormFile file, SportType sportType, MediaType mediaType);
    Task<MediaDeleteResponseDto> DeleteNewsMediaAsync(string s3Key);
    string GetNewsFolderPath(SportType sportType);
}

