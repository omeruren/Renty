using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Features.Brands.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;
using Renty.Server.Persistence;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class BrandEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RentyApiFactory _factory;

    public BrandEndpointsTests(RentyApiFactory factory) => _factory = factory;

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
    public async Task GetAllBrands_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/brands");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateBrand_WithoutManageFleetRole_Returns403()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Customer");

        var response = await client.PostAsJsonAsync("/api/v1/brands", new { Name = "Toyota", LogoUrl = (string?)null }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateBrand_AsManager_Returns201()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var payload = new { Name = $"Brand-{Guid.NewGuid():N}", LogoUrl = (string?)null };
        var response = await client.PostAsJsonAsync("/api/v1/brands", payload, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<BrandResponse>(JsonOptions);
        created!.Name.Should().Be(payload.Name);
    }

    [Fact]
    public async Task CreateBrand_DuplicateName_Returns409()
    {
        var brand = await SeedBrandAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var response = await client.PostAsJsonAsync("/api/v1/brands", new { Name = brand.Name, LogoUrl = (string?)null }, JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task DeleteBrand_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.DeleteAsync($"/api/v1/brands/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteBrand_ExistingBrandWithNoDependents_Returns204()
    {
        var brand = await SeedBrandAsync();
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.DeleteAsync($"/api/v1/brands/{brand.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
