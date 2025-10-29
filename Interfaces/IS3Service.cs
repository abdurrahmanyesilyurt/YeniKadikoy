using Kadikoy.DTOs;

namespace Kadikoy.Interfaces;

public interface IS3Service
{
    Task<MediaUploadResponseDto> UploadFileAsync(IFormFile file, string folder);
    Task<MediaDeleteResponseDto> DeleteFileAsync(string fileName, string folder);
    Task<List<string>> ListFilesAsync(string folder);
    string GetFileUrl(string fileName, string folder);
}

