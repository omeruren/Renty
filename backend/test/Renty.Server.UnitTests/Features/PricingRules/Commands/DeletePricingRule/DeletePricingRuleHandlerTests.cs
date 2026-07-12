using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.PricingRules.Commands.DeletePricingRule;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.PricingRules.Commands.DeletePricingRule;

public sealed class DeletePricingRuleHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesPricingRule()
    {
        var pricingRule = new PricingRuleBuilder().Build();
        var context = MockDbContextFactory.Create(pricingRules: [pricingRule]);
        var handler = new DeletePricingRuleHandler(context, CreateCurrentUserService(), NullLogger<DeletePricingRuleHandler>.Instance);

        await handler.Handle(new DeletePricingRuleCommand(pricingRule.Id), CancellationToken.None);

        context.PricingRules.Received(1).Remove(Arg.Is<Renty.Server.Domain.Entities.PricingRule>(p => p.Id == pricingRule.Id));
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownPricingRule_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new DeletePricingRuleHandler(context, CreateCurrentUserService(), NullLogger<DeletePricingRuleHandler>.Instance);

        var act = async () => await handler.Handle(new DeletePricingRuleCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }
}
