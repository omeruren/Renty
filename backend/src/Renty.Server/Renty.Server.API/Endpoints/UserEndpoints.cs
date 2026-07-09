using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Users.Commands.ActivateUser;
using Renty.Server.Application.Features.Users.Commands.AssignRoles;
using Renty.Server.Application.Features.Users.Commands.DeactivateUser;
using Renty.Server.Application.Features.Users.DTOs;
using Renty.Server.Application.Features.Users.Queries.GetAllUsers;
using Renty.Server.Application.Features.Users.Queries.GetUserById;

namespace Renty.Server.API.Endpoints;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/users").WithTags("Users").RequireAuthorization("AdminOnly");

        group.MapGet("/", GetAllUsers)
            .WithName("GetAllUsers")
            .Produces<PagedResponse<UserListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetUserById)
            .WithName("GetUserById")
            .Produces<UserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/activate", ActivateUser)
            .WithName("ActivateUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/deactivate", DeactivateUser)
            .WithName("DeactivateUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/roles", AssignRoles)
            .WithName("AssignRoles")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetAllUsers(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetAllUsersQuery(page, pageSize), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetUserById(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetUserByIdQuery(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> ActivateUser(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new ActivateUserCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateUser(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeactivateUserCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AssignRoles(
        ISender sender,
        Guid id,
        AssignRolesRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AssignRolesCommand(id, request.RoleIds), cancellationToken);
        return Results.NoContent();
    }
}

public sealed record AssignRolesRequest(List<Guid> RoleIds);
