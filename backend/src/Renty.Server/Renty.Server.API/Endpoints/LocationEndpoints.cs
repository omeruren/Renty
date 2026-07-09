using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Locations.Commands.CreateLocation;
using Renty.Server.Application.Features.Locations.Commands.DeleteLocation;
using Renty.Server.Application.Features.Locations.Commands.UpdateLocation;
using Renty.Server.Application.Features.Locations.DTOs;
using Renty.Server.Application.Features.Locations.Queries.GetAllLocations;
using Renty.Server.Application.Features.Locations.Queries.GetLocationById;

namespace Renty.Server.API.Endpoints;

public static class LocationEndpoints
{
    public static void MapLocationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/locations").WithTags("Locations").RequireAuthorization();

        group.MapGet("/", GetAllLocations)
            .WithName("GetAllLocations")
            .Produces<PagedResponse<LocationListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetLocationById)
            .WithName("GetLocationById")
            .Produces<LocationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateLocation)
            .WithName("CreateLocation")
            .RequireAuthorization("CanManageFleet")
            .Produces<LocationResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateLocation)
            .WithName("UpdateLocation")
            .RequireAuthorization("CanManageFleet")
            .Produces<LocationResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteLocation)
            .WithName("DeleteLocation")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetAllLocations(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        string? city = null,
        bool? isActive = null)
    {
        var result = await sender.Send(new GetAllLocationsQuery(page, pageSize, city, isActive), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetLocationById(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetLocationByIdQuery(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateLocation(
        ISender sender,
        CreateLocationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/locations/{result.Id}", result);
    }

    private static async Task<IResult> UpdateLocation(
        ISender sender,
        Guid id,
        UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateLocationCommand(
            id, request.Name, request.Address, request.City, request.District,
            request.PhoneNumber, request.Email, request.Latitude, request.Longitude, request.IsActive);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteLocation(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteLocationCommand(id), cancellationToken);
        return Results.NoContent();
    }
}

public sealed record UpdateLocationRequest(
    string Name,
    string Address,
    string City,
    string? District,
    string? PhoneNumber,
    string? Email,
    double? Latitude,
    double? Longitude,
    bool IsActive);
