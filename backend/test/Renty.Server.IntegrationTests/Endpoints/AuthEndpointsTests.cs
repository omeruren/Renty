using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Auth.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.IntegrationTests.Infrastructure;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Endpoints;

/// <summary>
/// The "auth" rate-limit policy (5 requests/minute) is shared by /register and /login and is
/// scoped to a single app host. xUnit constructs a new instance of this class per test method,
/// so each test gets its own RentyApiFactory/host/limiter-counter instead of sharing one across
/// the whole class — avoiding flaky 429s from unrelated tests exhausting the limiter.
/// </summary>
public sealed class AuthEndpointsTests : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RentyApiFactory _factory = new();

    public void Dispose() => _factory.Dispose();

    private async Task SeedCustomerRoleAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Roles.Add(new Role { Name = "Customer" });
        await context.SaveChangesAsync();
    }

    private async Task<(User User, string Password)> SeedUserAsync(bool isActive = true)
    {
        const string password = "Str0ng!Pass1";

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = passwordHasher.Hash(password),
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (user, password);
    }

    [Fact]
    public async Task Register_ValidCommand_Returns200WithAuthResponse()
    {
        await SeedCustomerRoleAsync();
        var client = _factory.CreateClient();

        var payload = new
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            Password = "Str0ng!Pass1",
            FirstName = "Jane",
            LastName = "Doe",
            PhoneNumber = (string?)null
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        auth!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        await SeedCustomerRoleAsync();
        var (existingUser, _) = await SeedUserAsync();
        var client = _factory.CreateClient();

        var payload = new
        {
            Email = existingUser.Email,
            Password = "Str0ng!Pass1",
            FirstName = "Jane",
            LastName = "Doe",
            PhoneNumber = (string?)null
        };

        var response = await client.PostAsJsonAsync("/api/v1/auth/register", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Login_ValidCredentials_Returns200WithAuthResponse()
    {
        var (user, password) = await SeedUserAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new { Email = user.Email, Password = password }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>(JsonOptions);
        auth!.AccessToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var (user, _) = await SeedUserAsync();
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/v1/auth/login", new { Email = user.Email, Password = "wrong-password" }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsync("/api/v1/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
