namespace Renty.Server.Infrastructure.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string SecretKey { get; init; } = default!;
    public string Issuer { get; init; } = default!;
    public string Audience { get; init; } = default!;
    public int AccessTokenExpirationMinutes { get; init; } = 15;
    public int RefreshTokenExpirationDays { get; init; } = 7;
}
