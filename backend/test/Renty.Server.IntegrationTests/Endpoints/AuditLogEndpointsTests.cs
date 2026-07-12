using System.Net;
using System.Text.Json;
using FluentAssertions;
using Renty.Server.IntegrationTests.Common;
using Renty.Server.IntegrationTests.Infrastructure;

namespace Renty.Server.IntegrationTests.Endpoints;

public sealed class AuditLogEndpointsTests : IClassFixture<RentyApiFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RentyApiFactory _factory;

    public AuditLogEndpointsTests(RentyApiFactory factory) => _factory = factory;

    [Fact]
    public async Task GetAllAuditLogs_WithoutAuth_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetAllAuditLogs_AsManager_Returns403()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Manager");

        var response = await client.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetAllAuditLogs_AsAdmin_Returns200()
    {
        var client = _factory.CreateClient();
        client.AuthenticateAs(Guid.NewGuid(), "Admin");

        var response = await client.GetAsync("/api/v1/audit-logs");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
