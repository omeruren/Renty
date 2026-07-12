using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class ReservationEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RentyApiFactory _factory;

    public ReservationEndpointsTests(RentyApiFactory factory) => _factory = factory;

    private sealed record ReservationFixture(User User, Car Car, Location Location);

    private async Task<ReservationFixture> SeedReservationPrerequisitesAsync(CarStatus carStatus = CarStatus.Available)
    {
        var brand = new Brand { Name = $"Brand-{Guid.NewGuid():N}" };
        var model = new Model { Name = $"Model-{Guid.NewGuid():N}", BrandId = brand.Id, Category = VehicleCategory.Sedan, SeatCount = 5 };
        var location = new Location { Name = $"Location-{Guid.NewGuid():N}", Address = "123 Main St", City = "Istanbul", IsActive = true };
        var car = new Car
        {
            LicensePlate = $"34-{Guid.NewGuid():N}"[..10],
            Year = 2023,
            Color = "Black",
            Mileage = 500,
            DailyPrice = 500m,
            Status = carStatus,
            BrandId = brand.Id,
            ModelId = model.Id,
            LocationId = location.Id
        };
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
        context.Brands.Add(brand);
        context.Models.Add(model);
        context.Locations.Add(location);
        context.Cars.Add(car);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        return new ReservationFixture(user, car, location);
    }

    [Fact]
    public async Task CreateReservation_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/reservations", new { }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateReservation_ValidRequest_Returns201AndBlocksCar()
    {
        var fixture = await SeedReservationPrerequisitesAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(fixture.User.Id, "Customer");

        var startDate = DateTime.UtcNow.AddDays(2);
        var endDate = startDate.AddDays(3);
        var payload = new
        {
            CarId = fixture.Car.Id,
            StartDate = startDate,
            EndDate = endDate,
            PickupLocationId = fixture.Location.Id,
            ReturnLocationId = fixture.Location.Id,
            Notes = "Integration test reservation"
        };

        var response = await client.PostAsJsonAsync("/api/v1/reservations", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<ReservationResponse>(JsonOptions);
        created.Should().NotBeNull();
        created!.Status.Should().Be(ReservationStatus.Pending);
        created.CarId.Should().Be(fixture.Car.Id);
        created.UserId.Should().Be(fixture.User.Id);

        var carResponse = await client.GetAsync($"/api/v1/cars/{fixture.Car.Id}");
        var car = await carResponse.Content.ReadFromJsonAsync<Renty.Server.Application.Features.Cars.DTOs.CarResponse>(JsonOptions);
        car!.Status.Should().Be(CarStatus.Reserved);
    }

    [Fact]
    public async Task CreateReservation_CarNotAvailable_Returns409()
    {
        var fixture = await SeedReservationPrerequisitesAsync(CarStatus.Rented);
        var client = _factory.CreateClient();
        client.AuthenticateAs(fixture.User.Id, "Customer");

        var startDate = DateTime.UtcNow.AddDays(2);
        var payload = new
        {
            CarId = fixture.Car.Id,
            StartDate = startDate,
            EndDate = startDate.AddDays(3),
            PickupLocationId = fixture.Location.Id,
            ReturnLocationId = fixture.Location.Id,
            Notes = (string?)null
        };

        var response = await client.PostAsJsonAsync("/api/v1/reservations", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateReservation_InvalidDateRange_Returns400()
    {
        var fixture = await SeedReservationPrerequisitesAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(fixture.User.Id, "Customer");

        var startDate = DateTime.UtcNow.AddDays(-1);
        var payload = new
        {
            CarId = fixture.Car.Id,
            StartDate = startDate,
            EndDate = startDate.AddDays(3),
            PickupLocationId = fixture.Location.Id,
            ReturnLocationId = fixture.Location.Id,
            Notes = (string?)null
        };

        var response = await client.PostAsJsonAsync("/api/v1/reservations", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAllReservations_WithoutManageReservationsRole_Returns403()
    {
        var fixture = await SeedReservationPrerequisitesAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(fixture.User.Id, "Customer");

        var response = await client.GetAsync("/api/v1/reservations");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMyReservations_AsAuthenticatedUser_Returns200()
    {
        var fixture = await SeedReservationPrerequisitesAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(fixture.User.Id, "Customer");

        var response = await client.GetAsync("/api/v1/reservations/my");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
