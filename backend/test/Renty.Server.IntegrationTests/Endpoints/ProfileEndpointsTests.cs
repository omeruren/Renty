using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Features.Profile.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class ProfileEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RentyApiFactory _factory;

    public ProfileEndpointsTests(RentyApiFactory factory) => _factory = factory;

    private async Task<User> SeedUserAsync()
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hashed",
            FirstName = "Test",
            LastName = "User",
            IsActive = true
        };

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task GetProfile_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/profile");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfile_AsAuthenticatedUser_Returns200()
    {
        var user = await SeedUserAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(user.Id, "Customer");

        var response = await client.GetAsync("/api/v1/profile");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(JsonOptions);
        profile!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task UpdateProfile_ValidPayload_Returns200WithUpdatedValues()
    {
        var user = await SeedUserAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(user.Id, "Customer");

        var payload = new { FirstName = "Updated", LastName = "Name", PhoneNumber = (string?)null, DateOfBirth = (DateOnly?)null };
        var response = await client.PutAsJsonAsync("/api/v1/profile", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var profile = await response.Content.ReadFromJsonAsync<ProfileResponse>(JsonOptions);
        profile!.FirstName.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateProfile_InvalidPayload_Returns400()
    {
        var user = await SeedUserAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(user.Id, "Customer");

        var payload = new { FirstName = "", LastName = "Name", PhoneNumber = (string?)null, DateOfBirth = (DateOnly?)null };
        var response = await client.PutAsJsonAsync("/api/v1/profile", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
