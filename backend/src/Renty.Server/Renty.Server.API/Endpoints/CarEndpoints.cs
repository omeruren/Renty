using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Cars.Commands.CreateCar;
using Renty.Server.Application.Features.Cars.Commands.DeleteCar;
using Renty.Server.Application.Features.Cars.Commands.UpdateCar;
using Renty.Server.Application.Features.Cars.DTOs;
using Renty.Server.Application.Features.Cars.Queries.GetAllCars;
using Renty.Server.Application.Features.Cars.Queries.GetCarById;
using Renty.Server.Domain.Enums;

namespace Renty.Server.API.Endpoints;

public static class CarEndpoints
{
    public static void MapCarEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/cars").WithTags("Cars").RequireAuthorization();

        group.MapGet("/", GetAllCars)
            .WithName("GetAllCars")
            .Produces<PagedResponse<CarListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetCarById)
            .WithName("GetCarById")
            .Produces<CarResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateCar)
            .WithName("CreateCar")
            .RequireAuthorization("CanManageFleet")
            .Produces<CarResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateCar)
            .WithName("UpdateCar")
            .RequireAuthorization("CanManageFleet")
            .Produces<CarResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeleteCar)
            .WithName("DeleteCar")
            .RequireAuthorization("CanManageFleet")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetAllCars(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        CarStatus? status = null,
        Guid? brandId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        string sortBy = "licensePlate",
        string sortOrder = "asc")
    {
        var query = new GetAllCarsQuery(page, pageSize, status, brandId, minPrice, maxPrice, sortBy, sortOrder);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCarById(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetCarByIdQuery(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateCar(
        ISender sender,
        CreateCarCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/cars/{result.Id}", result);
    }

    private static async Task<IResult> UpdateCar(
        ISender sender,
        Guid id,
        UpdateCarRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateCarCommand(
            id, request.Color, request.Mileage, request.DailyPrice, request.Status,
            request.Description, request.ImageUrl, request.LocationId);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteCar(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteCarCommand(id), cancellationToken);
        return Results.NoContent();
    }
}

public sealed record UpdateCarRequest(
    string Color,
    int Mileage,
    decimal DailyPrice,
    CarStatus Status,
    string? Description,
    string? ImageUrl,
    Guid LocationId);
