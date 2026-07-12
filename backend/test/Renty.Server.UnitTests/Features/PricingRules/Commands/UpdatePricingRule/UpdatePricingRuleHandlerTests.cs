using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.PricingRules.Commands.UpdatePricingRule;
using Renty.Server.Domain.Enums;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.PricingRules.Commands.UpdatePricingRule;

public sealed class UpdatePricingRuleHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedPricingRuleResponse()
    {
        var pricingRule = new PricingRuleBuilder().Build();
        var context = MockDbContextFactory.Create(pricingRules: [pricingRule]);
        var handler = new UpdatePricingRuleHandler(context, CreateCurrentUserService(), NullLogger<UpdatePricingRuleHandler>.Instance);
        var command = new UpdatePricingRuleCommand(
            pricingRule.Id, "Updated", PricingRuleType.SeasonalMultiplier, 1.5m, null, null, null, false, 2);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Updated");
        result.IsActive.Should().BeFalse();
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownPricingRule_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new UpdatePricingRuleHandler(context, CreateCurrentUserService(), NullLogger<UpdatePricingRuleHandler>.Instance);
        var command = new UpdatePricingRuleCommand(
            Guid.NewGuid(), "Updated", PricingRuleType.SeasonalMultiplier, 1.5m, null, null, null, false, 2);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
