using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Models.Commands.DeleteModel;
using Renty.Server.UnitTests.Common;
using Renty.Server.UnitTests.Common.Builders;

namespace Renty.Server.UnitTests.Features.Models.Commands.DeleteModel;

public sealed class DeleteModelHandlerTests
{
    private static ICurrentUserService CreateCurrentUserService()
    {
        var currentUserService = Substitute.For<ICurrentUserService>();
        currentUserService.UserId.Returns(Guid.NewGuid());
        currentUserService.IpAddress.Returns("127.0.0.1");
        return currentUserService;
    }

    [Fact]
    public async Task Handle_ValidCommand_RemovesModel()
    {
        var model = new ModelBuilder().Build();
        var context = MockDbContextFactory.Create(models: [model]);
        var handler = new DeleteModelHandler(context, CreateCurrentUserService(), NullLogger<DeleteModelHandler>.Instance);

        await handler.Handle(new DeleteModelCommand(model.Id), CancellationToken.None);

        context.Models.Received(1).Remove(Arg.Is<Renty.Server.Domain.Entities.Model>(m => m.Id == model.Id));
        await context.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_UnknownModel_ThrowsNotFoundException()
    {
        var context = MockDbContextFactory.Create();
        var handler = new DeleteModelHandler(context, CreateCurrentUserService(), NullLogger<DeleteModelHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteModelCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ModelHasCars_ThrowsBusinessRuleException()
    {
        var model = new ModelBuilder().Build();
        var car = new CarBuilder().WithModelId(model.Id).Build();
        var context = MockDbContextFactory.Create(models: [model], cars: [car]);
        var handler = new DeleteModelHandler(context, CreateCurrentUserService(), NullLogger<DeleteModelHandler>.Instance);

        var act = async () => await handler.Handle(new DeleteModelCommand(model.Id), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
