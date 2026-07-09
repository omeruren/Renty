using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Reservations.Commands.CancelReservation;
using Renty.Server.Application.Features.Reservations.Commands.CompleteReservation;
using Renty.Server.Application.Features.Reservations.Commands.ConfirmReservation;
using Renty.Server.Application.Features.Reservations.Commands.CreateReservation;
using Renty.Server.Application.Features.Reservations.Commands.UpdateReservation;
using Renty.Server.Application.Features.Reservations.DTOs;
using Renty.Server.Application.Features.Reservations.Queries.GetAllReservations;
using Renty.Server.Application.Features.Reservations.Queries.GetMyReservations;
using Renty.Server.Application.Features.Reservations.Queries.GetReservationById;
using Renty.Server.Domain.Enums;

namespace Renty.Server.API.Endpoints;

public static class ReservationEndpoints
{
    public static void MapReservationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/reservations").WithTags("Reservations").RequireAuthorization();

        group.MapGet("/", GetAllReservations)
            .WithName("GetAllReservations")
            .RequireAuthorization("CanManageReservations")
            .Produces<PagedResponse<ReservationListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/my", GetMyReservations)
            .WithName("GetMyReservations")
            .Produces<PagedResponse<ReservationListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetReservationById)
            .WithName("GetReservationById")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateReservation)
            .WithName("CreateReservation")
            .Produces<ReservationResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateReservation)
            .WithName("UpdateReservation")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/cancel", CancelReservation)
            .WithName("CancelReservation")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/confirm", ConfirmReservation)
            .WithName("ConfirmReservation")
            .RequireAuthorization("CanManageReservations")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/complete", CompleteReservation)
            .WithName("CompleteReservation")
            .RequireAuthorization("CanManageReservations")
            .Produces<ReservationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetAllReservations(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        ReservationStatus? status = null,
        Guid? carId = null,
        Guid? userId = null,
        string sortBy = "startDate",
        string sortOrder = "desc")
    {
        var query = new GetAllReservationsQuery(page, pageSize, status, carId, userId, sortBy, sortOrder);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetMyReservations(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        ReservationStatus? status = null)
    {
        var result = await sender.Send(new GetMyReservationsQuery(page, pageSize, status), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetReservationById(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetReservationByIdQuery(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateReservation(
        ISender sender,
        CreateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateReservationCommand(
            request.CarId, request.StartDate, request.EndDate,
            request.PickupLocationId, request.ReturnLocationId, request.Notes);

        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/reservations/{result.Id}", result);
    }

    private static async Task<IResult> UpdateReservation(
        ISender sender,
        Guid id,
        UpdateReservationRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReservationCommand(
            id, request.StartDate, request.EndDate,
            request.PickupLocationId, request.ReturnLocationId, request.Notes);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CancelReservation(
        ISender sender,
        Guid id,
        CancelReservationRequest? request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CancelReservationCommand(id, request?.Reason), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ConfirmReservation(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new ConfirmReservationCommand(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CompleteReservation(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CompleteReservationCommand(id), cancellationToken);
        return Results.Ok(result);
    }
}

public sealed record CreateReservationRequest(
    Guid CarId,
    DateTime StartDate,
    DateTime EndDate,
    Guid PickupLocationId,
    Guid ReturnLocationId,
    string? Notes);

public sealed record UpdateReservationRequest(
    DateTime StartDate,
    DateTime EndDate,
    Guid PickupLocationId,
    Guid ReturnLocationId,
    string? Notes);

public sealed record CancelReservationRequest(string? Reason);
