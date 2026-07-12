using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Models.Commands.UpdateModel;
using Renty.Server.Domain.Enums;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Models.Commands.UpdateModel;

public sealed class UpdateModelHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_ReturnsUpdatedModelResponse()
    {
        var brand = new BrandBuilder().Build();
        var model = new ModelBuilder().WithBrandId(brand.Id).Build();
        model.Brand = brand;
        var context = MockDbContextFactory.Create(brands: [brand], models: [model]);
        var handler = new UpdateModelHandler(context, CreateCurrentUserService(), NullLogger<UpdateModelHandler>.Instance);

        var command = new UpdateModelCommand(model.Id, "Camry", VehicleCategory.Sedan, 5, TransmissionType.Automatic, FuelType.Hybrid);

        var result = await handler.Handle(command, CancellationToken.None);

        result.Name.Should().Be("Camry");
        result.FuelType.Should().Be(FuelType.Hybrid);
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownModel_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new UpdateModelHandler(context, CreateCurrentUserService(), NullLogger<UpdateModelHandler>.Instance);
        var command = new UpdateModelCommand(Guid.NewGuid(), "Camry", VehicleCategory.Sedan, 5, TransmissionType.Automatic, FuelType.Hybrid);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_DuplicateNameWithinSameBrand_ThrowsBusinessRuleException()
    {
        var brand = new BrandBuilder().Build();
        var modelA = new ModelBuilder().WithBrandId(brand.Id).Build();
        modelA.Brand = brand;
        var modelB = new ModelBuilder().WithBrandId(brand.Id).Build();
        modelB.Brand = brand;
        var context = MockDbContextFactory.Create(brands: [brand], models: [modelA, modelB]);
        var handler = new UpdateModelHandler(context, CreateCurrentUserService(), NullLogger<UpdateModelHandler>.Instance);

        var command = new UpdateModelCommand(modelA.Id, modelB.Name, VehicleCategory.Sedan, 5, TransmissionType.Automatic, FuelType.Gasoline);

        var act = async () => await handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
