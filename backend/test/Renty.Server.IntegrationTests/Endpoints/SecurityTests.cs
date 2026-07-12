using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Renty.Server.IntegrationTests.Infrastructure;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class SecurityHeadersTests : IClassFixture<RentyApiFactory>
{
    private readonly RentyApiFactory _factory;

    public SecurityHeadersTests(RentyApiFactory factory) => _factory = factory;

    [Fact]
    public async Task AnyResponse_IncludesSecurityHeaders()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.Headers.GetValues("X-Content-Type-Options").Should().Contain("nosniff");
        response.Headers.GetValues("X-Frame-Options").Should().Contain("DENY");
        response.Headers.GetValues("Referrer-Policy").Should().Contain("strict-origin-when-cross-origin");
        response.Headers.GetValues("Content-Security-Policy").Should().Contain("default-src 'self'");
        response.Headers.GetValues("Permissions-Policy").Should().Contain("geolocation=(), camera=(), microphone=()");
    }
}

public sealed class CorsTests : IClassFixture<RentyApiFactory>
{
    private readonly RentyApiFactory _factory;

    public CorsTests(RentyApiFactory factory) => _factory = factory;

    [Fact]
    public async Task AllowedOrigin_GetsAccessControlAllowOriginHeader()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("Origin", "http://localhost:5173");

        var response = await client.SendAsync(request);

        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("http://localhost:5173");
    }

    [Fact]
    public async Task DisallowedOrigin_DoesNotGetAccessControlAllowOriginHeader()
    {
        var client = _factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Get, "/health/live");
        request.Headers.Add("Origin", "https://evil.example.com");

        var response = await client.SendAsync(request);

        response.Headers.Contains("Access-Control-Allow-Origin").Should().BeFalse();
    }
}

public sealed class RateLimitingTests : IDisposable
{
    private readonly RentyApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    [Fact]
    public async Task ExceedingWriteLimit_Returns429()
    {
        var client = _factory.CreateClient();
        var payload = new { Name = "Test", LogoUrl = (string?)null };

        HttpResponseMessage? last = null;
        for (var i = 0; i < 31; i++)
            last = await client.PostAsJsonAsync("/api/v1/brands", payload);

        last!.StatusCode.Should().Be((HttpStatusCode)429);
    }

    [Fact]
    public async Task AuthEndpoints_AreExemptFromGlobalWriteLimit()
    {
        var client = _factory.CreateClient();

        // The "auth" named limiter (5/min) kicks in well before 31 requests would, proving the
        // global "write" limiter's exemption for /api/v1/auth/* doesn't leave it unlimited —
        // it's still governed by its own stricter policy, just not double-counted.
        HttpResponseMessage? last = null;
        for (var i = 0; i < 6; i++)
            last = await client.PostAsJsonAsync("/api/v1/auth/login", new { Email = "nobody@example.com", Password = "x" });

        last!.StatusCode.Should().Be((HttpStatusCode)429);
    }
}
