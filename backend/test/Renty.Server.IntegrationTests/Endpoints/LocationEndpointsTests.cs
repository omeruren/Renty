using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Renty.Server.Application.Features.Locations.DTOs;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class LocationEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RentyApiFactory _factory;

    public LocationEndpointsTests(RentyApiFactory factory) => _factory = factory;

    [Fact]
    public async Task CreateLocation_AsManager_Returns201()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new
        {
            Name = $"Location-{Guid.NewGuid():N}",
            Address = "123 Main St",
            City = "Istanbul",
            District = (string?)null,
            PhoneNumber = (string?)null,
            Email = (string?)null,
            Latitude = (double?)null,
            Longitude = (double?)null
        };

        var response = await client.PostAsJsonAsync("/api/v1/locations", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<LocationResponse>(JsonOptions);
        created!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task CreateLocation_InvalidEmail_Returns400()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new
        {
            Name = "Downtown",
            Address = "123 Main St",
            City = "Istanbul",
            District = (string?)null,
            PhoneNumber = (string?)null,
            Email = "not-an-email",
            Latitude = (double?)null,
            Longitude = (double?)null
        };

        var response = await client.PostAsJsonAsync("/api/v1/locations", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task DeleteLocation_AsManagerOnly_Returns403()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var response = await client.DeleteAsync($"/api/v1/locations/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteLocation_AsAdmin_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.DeleteAsync($"/api/v1/locations/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
