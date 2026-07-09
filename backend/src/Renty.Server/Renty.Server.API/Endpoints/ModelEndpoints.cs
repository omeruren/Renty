using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Models.Commands.CreateModel;
using Renty.Server.Application.Features.Models.Commands.DeleteModel;
using Renty.Server.Application.Features.Models.Commands.UpdateModel;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Application.Features.Models.Queries.GetAllModels;
using Renty.Server.Application.Features.Models.Queries.GetModelById;
using Renty.Server.Domain.Enums;

namespace Renty.Server.API.Endpoints;

public static class ModelEndpoints
{
    public static void MapModelEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/models").WithTags("Models").RequireAuthorization();

        group.MapGet("/", GetAllModels)
            .WithName("GetAllModels")
            .Produces<PagedResponse<ModelListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetModelById)
            .WithName("GetModelById")
            .Produces<ModelResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateModel)
            .WithName("CreateModel")
            .RequireAuthorization("CanManageFleet")
            .Produces<ModelResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateModel)
            .WithName("UpdateModel")
            .RequireAuthorization("CanManageFleet")
            .Produces<ModelResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteModel)
            .WithName("DeleteModel")
            .RequireAuthorization("CanManageFleet")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetAllModels(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        Guid? brandId = null)
    {
        var result = await sender.Send(new GetAllModelsQuery(page, pageSize, brandId), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetModelById(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetModelByIdQuery(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateModel(
        ISender sender,
        CreateModelCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/models/{result.Id}", result);
    }

    private static async Task<IResult> UpdateModel(
        ISender sender,
        Guid id,
        UpdateModelRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateModelCommand(
            id, request.Name, request.Category, request.SeatCount, request.TransmissionType, request.FuelType);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteModel(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteModelCommand(id), cancellationToken);
        return Results.NoContent();
    }
}

public sealed record UpdateModelRequest(
    string Name,
    VehicleCategory Category,
    int SeatCount,
    TransmissionType TransmissionType,
    FuelType FuelType);
