using JwtAuthDotNet9.Data;
using JwtAuthDotNet9.Entities;
using JwtAuthDotNet9.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace JwtAuthDotNet9.Services
{
    public class AuthService(UserDbContext context, IConfiguration configuration) : IAuthService
    {
        public async Task<TokenResponseDto?> LoginAsync(UserDto request, string? clientIp = null)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (user is null)
            {
                return null;
            }
            if (new PasswordHasher<User>().VerifyHashedPassword(user, user.PasswordHash, request.Password)
                == PasswordVerificationResult.Failed)
            {
                return null;
            }

            // Invalidar sesiones de diferentes IPs
            await InvalidateSessionsFromOtherIpsAsync(user.Id, clientIp);

            // Crear o reutilizar sesión para esta IP
            var session = await CreateOrUpdateSessionAsync(user.Id, clientIp);

            return await CreateTokenResponse(user, session);
        }

        private async Task<TokenResponseDto> CreateTokenResponse(User user, UserSession session)
        {
            return new TokenResponseDto
            {
                AccessToken = CreateToken(user, session),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(session)
            };
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            if (await context.Users.AnyAsync(u => u.Username == request.Username))
            {
                return null;
            }

            var user = new User();
            var hashedPassword = new PasswordHasher<User>()
                .HashPassword(user, request.Password);

            user.Username = request.Username;
            user.PasswordHash = hashedPassword;
            user.Role = "User"; // Rol por defecto

            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var session = await ValidateRefreshTokenAsync(request.UserId, request.RefreshToken);
            if (session is null)
                return null;

            var user = await context.Users.FindAsync(session.UserId);
            if (user is null)
                return null;

            return await CreateTokenResponse(user, session);
        }

        private async Task<UserSession?> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
        {
            var session = await context.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.RefreshToken == refreshToken && s.IsActive);
            
            if (session is null || session.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                return null;
            }

            return session;
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(UserSession session)
        {
            var refreshToken = GenerateRefreshToken();
            session.RefreshToken = refreshToken;
            session.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await context.SaveChangesAsync();
            return refreshToken;
        }

        private async Task InvalidateSessionsFromOtherIpsAsync(Guid userId, string? clientIp)
        {
            // Invalidar sesiones activas de diferentes IPs
            var sessionsToInvalidate = await context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive && s.IpAddress != clientIp)
                .ToListAsync();

            foreach (var session in sessionsToInvalidate)
            {
                session.IsActive = false;
                session.RefreshToken = null;
                session.RefreshTokenExpiryTime = null;
            }

            await context.SaveChangesAsync();
        }

        private async Task<UserSession> CreateOrUpdateSessionAsync(Guid userId, string? clientIp)
        {
            // Siempre crear una nueva sesión para permitir múltiples sesiones por IP
            var newSession = new UserSession
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                SessionId = Guid.NewGuid().ToString(),
                IpAddress = clientIp,
                CreatedAt = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IsActive = true
            };
            
            context.UserSessions.Add(newSession);
            await context.SaveChangesAsync();
            return newSession;
        }

        public async Task<bool> IsSessionValidAsync(Guid userId, string sessionId)
        {
            var session = await context.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionId == sessionId && s.IsActive);
            return session is not null;
        }

        public async Task LogoutAsync(Guid userId)
        {
            // Invalidar todas las sesiones del usuario
            var sessions = await context.UserSessions
                .Where(s => s.UserId == userId && s.IsActive)
                .ToListAsync();

            foreach (var session in sessions)
            {
                session.IsActive = false;
                session.RefreshToken = null;
                session.RefreshTokenExpiryTime = null;
            }

            await context.SaveChangesAsync();
        }

        public async Task LogoutAsync(Guid userId, string sessionId)
        {
            // Invalidar solo la sesión especifica
            var session = await context.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionId == sessionId);

            if (session is not null)
            {
                session.IsActive = false;
                session.RefreshToken = null;
                session.RefreshTokenExpiryTime = null;
                await context.SaveChangesAsync();
            }
        }

        private string CreateToken(User user, UserSession session)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("SessionId", session.SessionId)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(configuration.GetValue<string>("AppSettings:Token")!));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }
    }
}
