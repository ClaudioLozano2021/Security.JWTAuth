using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;

namespace JwtAuthDotNet9.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(UserDto request, string? clientIp = null);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task<bool> IsSessionValidAsync(Guid userId, string sessionId);
        Task LogoutAsync(Guid userId);
        Task LogoutAsync(Guid userId, string sessionId);
    }
}
