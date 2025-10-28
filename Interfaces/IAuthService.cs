using Kadikoy.DTOs;

namespace Kadikoy.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto loginDto);
    Task<string> GenerateJwtToken(int userId, string username, List<string> roles);
}

