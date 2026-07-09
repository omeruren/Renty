using MediatR;
using Renty.Server.Application.Features.Profile.Commands.UpdateProfile;
using Renty.Server.Application.Features.Profile.DTOs;
using Renty.Server.Application.Features.Profile.Queries.GetProfile;

namespace Renty.Server.API.Endpoints;

public static class ProfileEndpoints
{
    public static void MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/profile").WithTags("Profile").RequireAuthorization();

        group.MapGet("/", GetProfile)
            .WithName("GetProfile")
            .Produces<ProfileResponse>(StatusCodes.Status200OK);

        group.MapPut("/", UpdateProfile)
            .WithName("UpdateProfile")
            .Produces<ProfileResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> GetProfile(ISender sender, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetProfileQuery(), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> UpdateProfile(
        ISender sender,
        UpdateProfileCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }
}
