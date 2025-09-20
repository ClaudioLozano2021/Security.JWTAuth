namespace ProtectedApi.Services
{
    public interface IAuthService
    {
        Task<bool> IsSessionValidAsync(Guid userId, string sessionId);
    }
}