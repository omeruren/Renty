using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class ModelEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RentyApiFactory _factory;

    public ModelEndpointsTests(RentyApiFactory factory) => _factory = factory;

    private async Task<Brand> SeedBrandAsync()
    {
        var brand = new Brand { Name = $"Brand-{Guid.NewGuid():N}" };

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Brands.Add(brand);
        await context.SaveChangesAsync();

        return brand;
    }

    [Fact]
    public async Task CreateModel_UnknownBrand_Returns404()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new
        {
            Name = "Corolla",
            Category = VehicleCategory.Sedan,
            SeatCount = 5,
            TransmissionType = TransmissionType.Automatic,
            FuelType = FuelType.Gasoline,
            BrandId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/api/v1/models", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateModel_AsManager_Returns201()
    {
        var brand = await SeedBrandAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new
        {
            Name = $"Model-{Guid.NewGuid():N}",
            Category = VehicleCategory.Sedan,
            SeatCount = 5,
            TransmissionType = TransmissionType.Automatic,
            FuelType = FuelType.Gasoline,
            BrandId = brand.Id
        };

        var response = await client.PostAsJsonAsync("/api/v1/models", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ModelResponse>(JsonOptions);
        created!.BrandId.Should().Be(brand.Id);
    }

    [Fact]
    public async Task CreateModel_WithoutManageFleetRole_Returns403()
    {
        var brand = await SeedBrandAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Customer");

        var payload = new
        {
            Name = $"Model-{Guid.NewGuid():N}",
            Category = VehicleCategory.Sedan,
            SeatCount = 5,
            TransmissionType = TransmissionType.Automatic,
            FuelType = FuelType.Gasoline,
            BrandId = brand.Id
        };

        var response = await client.PostAsJsonAsync("/api/v1/models", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetModelById_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Customer");

        var response = await client.GetAsync($"/api/v1/models/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
