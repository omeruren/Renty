using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Domain.Enums;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class PricingRuleEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly RentyApiFactory _factory;

    public PricingRuleEndpointsTests(RentyApiFactory factory) => _factory = factory;

    private static object ValidPayload() => new
    {
        Name = $"Rule-{Guid.NewGuid():N}",
        RuleType = PricingRuleType.WeekendMultiplier,
        Multiplier = 1.2m,
        StartDate = (DateTime?)null,
        EndDate = (DateTime?)null,
        VehicleCategory = (VehicleCategory?)null,
        Priority = 1
    };

    [Fact]
    public async Task GetAllPricingRules_AsCustomer_Returns403()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Customer");

        var response = await client.GetAsync("/api/v1/pricing-rules");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePricingRule_AsManagerOnly_Returns403()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var response = await client.PostAsJsonAsync("/api/v1/pricing-rules", ValidPayload(), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePricingRule_AsAdmin_Returns201()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.PostAsJsonAsync("/api/v1/pricing-rules", ValidPayload(), JsonOptions);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await response.Content.ReadFromJsonAsync<PricingRuleResponse>(JsonOptions);
        created!.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetPricingRuleById_AsAdmin_UnknownId_Returns404()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.GetAsync($"/api/v1/pricing-rules/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
