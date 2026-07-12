using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Renty.Server.Application.Features.Reports.DTOs;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class ReportEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RentyApiFactory _factory;

    public ReportEndpointsTests(RentyApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetRevenueReport_AsCustomer_Returns403()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Customer");

        var response = await client.GetAsync("/api/v1/reports/revenue");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetRevenueReport_AsManager_Returns200()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var response = await client.GetAsync("/api/v1/reports/revenue");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var report = await response.Content.ReadFromJsonAsync<RevenueReportResponse>(JsonOptions);
        report.Should().NotBeNull();
    }

    [Fact]
    public async Task GetRevenueReport_ToBeforeFrom_Returns400()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var response = await client.GetAsync("/api/v1/reports/revenue?from=2026-03-10&to=2026-03-01");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
