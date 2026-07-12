using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Models.Commands.CreateModel;
using Renty.Server.Domain.Enums;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Models.Commands.CreateModel;

public sealed class CreateModelHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    private static CreateModelCommand BuildCommand(Guid brandId, string name = "Corolla") =>
        new(name, VehicleCategory.Sedan, 5, TransmissionType.Automatic, FuelType.Gasoline, brandId);

    [Fact]
    public async Task Handle_ValidCommand_ReturnsModelResponse()
    {
        var brand = new BrandBuilder().Build();
        var context = MockDbContextFactory.Create(brands: [brand]);
        var handler = new CreateModelHandler(context, CreateCurrentUserService(), NullLogger<CreateModelHandler>.Instance);

        var result = await handler.Handle(BuildCommand(brand.Id), CancellationToken.None);

        result.Name.Should().Be("Corolla");
        result.BrandId.Should().Be(brand.Id);
        result.BrandName.Should().Be(brand.Name);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownBrand_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new CreateModelHandler(context, CreateCurrentUserService(), NullLogger<CreateModelHandler>.Instance);

        var act = async () => await handler.Handle(BuildCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DuplicateNameWithinSameBrand_ThrowsBusinessRuleException()
    {
        var brand = new BrandBuilder().Build();
        var existingModel = new ModelBuilder().WithBrandId(brand.Id).Build();
        var context = MockDbContextFactory.Create(brands: [brand], models: [existingModel]);
        var handler = new CreateModelHandler(context, CreateCurrentUserService(), NullLogger<CreateModelHandler>.Instance);

        var act = async () => await handler.Handle(BuildCommand(brand.Id, existingModel.Name), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
