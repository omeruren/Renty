using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Domain.Entities;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class UserEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RentyApiFactory _factory;

    public UserEndpointsTests(RentyApiFactory factory) => _factory = factory;

    private async Task<User> SeedUserAsync(bool isActive = true)
    {
        var user = new User
        {
            Email = $"{Guid.NewGuid():N}@example.com",
            PasswordHash = "hashed",
            FirstName = "Test",
            LastName = "User",
            IsActive = isActive
        };

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    [Fact]
    public async Task GetAllUsers_AsManager_Returns403()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var response = await client.GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllUsers_AsAdmin_Returns200()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.GetAsync("/api/v1/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ActivateUser_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.PutAsync($"/api/v1/users/{Guid.NewGuid()}/activate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeactivateUser_ExistingUser_Returns204()
    {
        var user = await SeedUserAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.PutAsync($"/api/v1/users/{user.Id}/deactivate", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
