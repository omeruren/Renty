using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class CarEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RentyApiFactory _factory;

    public CarEndpointsTests(RentyApiFactory factory) => _factory = factory;

    private async Task<(Brand Brand, Model Model, Location Location)> SeedFleetPrerequisitesAsync()
    {
        var brand = new Brand { Name = $"Brand-{Guid.NewGuid():N}" };
        var model = new Model { Name = $"Model-{Guid.NewGuid():N}", BrandId = brand.Id, Category = VehicleCategory.Sedan, SeatCount = 5 };
        var location = new Location { Name = $"Location-{Guid.NewGuid():N}", Address = "123 Main St", City = "Istanbul", IsActive = true };

        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Brands.Add(brand);
        context.Models.Add(model);
        context.Locations.Add(location);
        await context.SaveChangesAsync();

        return (brand, model, location);
    }

    [Fact]
    public async Task GetAllCars_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/cars");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllCars_AsAuthenticatedUser_Returns200()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Customer");

        var response = await client.GetAsync("/api/v1/cars");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCar_WithoutManageFleetRole_Returns403()
    {
        var (brand, model, location) = await SeedFleetPrerequisitesAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Customer");

        var payload = new
        {
            LicensePlate = "34XYZ999",
            Year = 2024,
            Color = "White",
            Mileage = 0,
            DailyPrice = 900m,
            Description = (string?)null,
            ImageUrl = (string?)null,
            BrandId = brand.Id,
            ModelId = model.Id,
            LocationId = location.Id
        };

        var response = await client.PostAsJsonAsync("/api/v1/cars", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateCar_AsManager_Returns201AndPersistsCar()
    {
        var (brand, model, location) = await SeedFleetPrerequisitesAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new
        {
            LicensePlate = $"34-{Guid.NewGuid():N}"[..10],
            Year = 2024,
            Color = "White",
            Mileage = 0,
            DailyPrice = 900m,
            Description = (string?)null,
            ImageUrl = (string?)null,
            BrandId = brand.Id,
            ModelId = model.Id,
            LocationId = location.Id
        };

        var response = await client.PostAsJsonAsync("/api/v1/cars", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<CarResponse>(JsonOptions);
        created.Should().NotBeNull();
        created!.Status.Should().Be(CarStatus.Available);
        created.BrandId.Should().Be(brand.Id);

        var getResponse = await client.GetAsync($"/api/v1/cars/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateCar_InvalidPayload_Returns400()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new
        {
            LicensePlate = "",
            Year = 2024,
            Color = "White",
            Mileage = 0,
            DailyPrice = 900m,
            Description = (string?)null,
            ImageUrl = (string?)null,
            BrandId = Guid.NewGuid(),
            ModelId = Guid.NewGuid(),
            LocationId = Guid.NewGuid()
        };

        var response = await client.PostAsJsonAsync("/api/v1/cars", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateCar_DuplicateLicensePlate_Returns409()
    {
        var (brand, model, location) = await SeedFleetPrerequisitesAsync();
        var licensePlate = $"34-{Guid.NewGuid():N}"[..10];

        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            context.Cars.Add(new Car
            {
                LicensePlate = licensePlate,
                Year = 2023,
                Color = "Black",
                Mileage = 500,
                DailyPrice = 600m,
                Status = CarStatus.Available,
                BrandId = brand.Id,
                ModelId = model.Id,
                LocationId = location.Id
            });
            await context.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new
        {
            LicensePlate = licensePlate,
            Year = 2024,
            Color = "White",
            Mileage = 0,
            DailyPrice = 900m,
            Description = (string?)null,
            ImageUrl = (string?)null,
            BrandId = brand.Id,
            ModelId = model.Id,
            LocationId = location.Id
        };

        var response = await client.PostAsJsonAsync("/api/v1/cars", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
