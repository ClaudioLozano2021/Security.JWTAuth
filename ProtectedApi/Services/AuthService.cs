using Microsoft.EntityFrameworkCore;
using ProtectedApi.Data;

namespace ProtectedApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext context;

        public AuthService(UserDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> IsSessionValidAsync(Guid userId, string sessionId)
        {
            var session = await context.UserSessions
                .FirstOrDefaultAsync(s => s.UserId == userId && s.SessionId == sessionId && s.IsActive);

            return session != null;
        }
    }
}