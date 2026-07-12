using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Brands.Commands.CreateBrand;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Brands.Commands.CreateBrand;

public sealed class CreateBrandHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsBrandResponse()
    {
        var context = MockDbContextFactory.Create();
        var handler = new CreateBrandHandler(context, CreateCurrentUserService(), NullLogger<CreateBrandHandler>.Instance);
        var command = new CreateBrandCommand("Toyota", "https://example.com/logo.png");

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Toyota");
        result.LogoUrl.Should().Be("https://example.com/logo.png");
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsBusinessRuleException()
    {
        var existingBrand = new BrandBuilder().WithName("Toyota").Build();
        var context = MockDbContextFactory.Create(brands: [existingBrand]);
        var handler = new CreateBrandHandler(context, CreateCurrentUserService(), NullLogger<CreateBrandHandler>.Instance);
        var command = new CreateBrandCommand("Toyota", null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*already exists*");
    }
}
