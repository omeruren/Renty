using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Cars.Commands.CreateCar;
using Renty.Server.Domain.Enums;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Cars.Commands.CreateCar;

public sealed class CreateCarHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService(Guid? userId = null)
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(userId ?? Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    private static CreateCarCommand BuildCommand(Guid brandId, Guid modelId, Guid locationId, string licensePlate = "34ABC123") =>
        new(licensePlate, 2024, "Black", 1500, 850m, "A well maintained car", null, brandId, modelId, locationId);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsCarResponse()
    {
        var brand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(brand.Id).Build();
        var location = new LocationBuilder().Build();

        var context = MockDbContextFactory.Create(brands: [brand], models: [model], locations: [location]);
        var handler = new CreateCarHandler(context, CreateCurrentUserService(), NullLogger<CreateCarHandler>.Instance);
        var command = BuildCommand(brand.Id, model.Id, location.Id);

        var result = await handler.Handle(command, CancellationToken.None);

        result.LicensePlate.Should().Be(command.LicensePlate);
        result.Status.Should().Be(CarStatus.Available);
        result.BrandId.Should().Be(brand.Id);
        result.BrandName.Should().Be(brand.Name);
        result.ModelId.Should().Be(model.Id);
        result.ModelName.Should().Be(model.Name);
        result.LocationId.Should().Be(location.Id);
        result.LocationName.Should().Be(location.Name);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownBrand_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new CreateCarHandler(context, CreateCurrentUserService(), NullLogger<CreateCarHandler>.Instance);
        var command = BuildCommand(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_UnknownModel_ThrowsNotFoundException()
    {
        var brand = new BrandBuilder().Build();
        var location = new LocationBuilder().Build();

        var context = MockDbContextFactory.Create(brands: [brand], locations: [location]);
        var handler = new CreateCarHandler(context, CreateCurrentUserService(), NullLogger<CreateCarHandler>.Instance);
        var command = BuildCommand(brand.Id, Guid.NewGuid(), location.Id);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ModelBelongsToDifferentBrand_ThrowsBusinessRuleException()
    {
        var brand = new BrandBuilder().Build();
        var otherBrand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(otherBrand.Id).Build();
        var location = new LocationBuilder().Build();

        var context = MockDbContextFactory.Create(brands: [brand, otherBrand], models: [model], locations: [location]);
        var handler = new CreateCarHandler(context, CreateCurrentUserService(), NullLogger<CreateCarHandler>.Instance);
        var command = BuildCommand(brand.Id, model.Id, location.Id);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*does not belong*");
    }

    [Fact]
    public async Task Handle_UnknownLocation_ThrowsNotFoundException()
    {
        var brand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(brand.Id).Build();

        var context = MockDbContextFactory.Create(brands: [brand], models: [model]);
        var handler = new CreateCarHandler(context, CreateCurrentUserService(), NullLogger<CreateCarHandler>.Instance);
        var command = BuildCommand(brand.Id, model.Id, Guid.NewGuid());

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DuplicateLicensePlate_ThrowsBusinessRuleException()
    {
        var brand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(brand.Id).Build();
        var location = new LocationBuilder().Build();
        var existingCar = new CarBuilder()
            .WithLicensePlate("34ABC123")
            .WithBrandId(brand.Id)
            .WithModelId(model.Id)
            .WithLocationId(location.Id)
            .Build();

        var context = MockDbContextFactory.Create(brands: [brand], models: [model], locations: [location], cars: [existingCar]);
        var handler = new CreateCarHandler(context, CreateCurrentUserService(), NullLogger<CreateCarHandler>.Instance);
        var command = BuildCommand(brand.Id, model.Id, location.Id, "34ABC123");

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>()
            .WithMessage("*license plate already exists*");
    }
}
