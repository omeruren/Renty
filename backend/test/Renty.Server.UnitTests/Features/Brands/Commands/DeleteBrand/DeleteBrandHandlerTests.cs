using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Brands.Commands.DeleteBrand;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Brands.Commands.DeleteBrand;

public sealed class DeleteBrandHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesBrand()
    {
        var brand = new BrandBuilder().Build();
        var context = MockDbContextFactory.Create(brands: [brand]);
        var handler = new DeleteBrandHandler(context, CreateCurrentUserService(), NullLogger<DeleteBrandHandler>.Instance);

        await handler.Handle(new DeleteBrandCommand(brand.Id), CancellationToken.None);

        context.Brands.Received(1).Remove(Arg.Is<Renty.Server.Domain.Entities.Brand>(b => b.Id == brand.Id));
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownBrand_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new DeleteBrandHandler(context, CreateCurrentUserService(), NullLogger<DeleteBrandHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteBrandCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_BrandHasModels_ThrowsBusinessRuleException()
    {
        var brand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(brand.Id).Build();
        var context = MockDbContextFactory.Create(brands: [brand], models: [model]);
        var handler = new DeleteBrandHandler(context, CreateCurrentUserService(), NullLogger<DeleteBrandHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteBrandCommand(brand.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*existing models*");
    }

    [Fact]
    public async Task Handle_BrandHasCars_ThrowsBusinessRuleException()
    {
        var brand = new BrandBuilder().Build();
        var car = new CarBuilder().WithBrandId(brand.Id).Build();
        var context = MockDbContextFactory.Create(brands: [brand], cars: [car]);
        var handler = new DeleteBrandHandler(context, CreateCurrentUserService(), NullLogger<DeleteBrandHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteBrandCommand(brand.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*existing cars*");
    }
}
