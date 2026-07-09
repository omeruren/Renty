using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Infrastructure.Configuration;

namespace Renty.Server.Infrastructure.Services;

public sealed class JwtTokenService(IOptions<JwtSettings> jwtOptions) : IJwtTokenService
{
    private readonly JwtSettings _settings = jwtOptions.Value;

    public TimeSpan RefreshTokenLifetime => TimeSpan.FromDays(_settings.RefreshTokenExpirationDays);

    public (string Token, DateTime ExpiresAt) GenerateAccessToken(
        Guid userId,
        string email,
        string fullName,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        List<Claim> claims =
        [
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(JwtRegisteredClaimNames.Email, email),
            new("name", fullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        ];

        claims.AddRange(roles.Select(role => new Claim("roles", role)));
        claims.AddRange(permissions.Select(permission => new Claim("permissions", permission)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(randomBytes);
    }

    public string HashToken(string token)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(hash);
    }
}
