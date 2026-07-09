namespace Renty.Server.Application.Common.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateAccessToken(
        Guid userId,
        string email,
        string fullName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);

    string GenerateRefreshToken();

    string HashToken(string token);

    TimeSpan RefreshTokenLifetime { get; }
}
