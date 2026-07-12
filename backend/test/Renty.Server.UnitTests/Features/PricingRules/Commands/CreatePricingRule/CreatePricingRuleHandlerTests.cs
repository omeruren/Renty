using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.PricingRules.Commands.CreatePricingRule;
using Renty.Server.Domain.Enums;
using Renty.Server.UnitTests.Common;

namespace Renty.Server.UnitTests.Features.PricingRules.Commands.CreatePricingRule;

public sealed class CreatePricingRuleHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsActivePricingRuleResponse()
    {
        var context = MockDbContextFactory.Create();
        var handler = new CreatePricingRuleHandler(context, CreateCurrentUserService(), NullLogger<CreatePricingRuleHandler>.Instance);
        var command = new CreatePricingRuleCommand("Weekend Surcharge", PricingRuleType.WeekendMultiplier, 1.2m, null, null, null, 1);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Weekend Surcharge");
        result.IsActive.Should().BeTrue();
        result.Multiplier.Should().Be(1.2m);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
