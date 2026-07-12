using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Brands.Commands.UpdateBrand;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Brands.Commands.UpdateBrand;

public sealed class UpdateBrandHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedBrandResponse()
    {
        var brand = new BrandBuilder().WithName("Old Name").Build();
        var context = MockDbContextFactory.Create(brands: [brand]);
        var handler = new UpdateBrandHandler(context, CreateCurrentUserService(), NullLogger<UpdateBrandHandler>.Instance);
        var command = new UpdateBrandCommand(brand.Id, "New Name", null);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("New Name");
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownBrand_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new UpdateBrandHandler(context, CreateCurrentUserService(), NullLogger<UpdateBrandHandler>.Instance);
        var command = new UpdateBrandCommand(Guid.NewGuid(), "New Name", null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_NameTakenByAnotherBrand_ThrowsBusinessRuleException()
    {
        var brand = new BrandBuilder().WithName("Honda").Build();
        var otherBrand = new BrandBuilder().WithName("Toyota").Build();
        var context = MockDbContextFactory.Create(brands: [brand, otherBrand]);
        var handler = new UpdateBrandHandler(context, CreateCurrentUserService(), NullLogger<UpdateBrandHandler>.Instance);
        var command = new UpdateBrandCommand(brand.Id, "Toyota", null);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
