using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.Brands.Commands.CreateBrand;
using Renty.Server.Application.Features.Brands.Commands.DeleteBrand;
using Renty.Server.Application.Features.Brands.Commands.UpdateBrand;
using Renty.Server.Application.Features.Brands.DTOs;
using Renty.Server.Application.Features.Brands.Queries.GetAllBrands;
using Renty.Server.Application.Features.Brands.Queries.GetBrandById;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Application.Features.Models.Queries.GetAllModels;

namespace Renty.Server.API.Endpoints;

public static class BrandEndpoints
{
    public static void MapBrandEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/brands").WithTags("Brands").RequireAuthorization();

        group.MapGet("/", GetAllBrands)
            .WithName("GetAllBrands")
            .Produces<PagedResponse<BrandListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetBrandById)
            .WithName("GetBrandById")
            .Produces<BrandResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}/models", GetModelsByBrand)
            .WithName("GetModelsByBrand")
            .Produces<PagedResponse<ModelListResponse>>(StatusCodes.Status200OK);

        group.MapPost("/", CreateBrand)
            .WithName("CreateBrand")
            .RequireAuthorization("CanManageFleet")
            .Produces<BrandResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateBrand)
            .WithName("UpdateBrand")
            .RequireAuthorization("CanManageFleet")
            .Produces<BrandResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{id:guid}", DeleteBrand)
            .WithName("DeleteBrand")
            .RequireAuthorization("CanManageFleet")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<IResult> GetAllBrands(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetAllBrandsQuery(page, pageSize), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetBrandById(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetBrandByIdQuery(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetModelsByBrand(
        ISender sender,
        Guid id,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10)
    {
        var result = await sender.Send(new GetAllModelsQuery(page, pageSize, id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreateBrand(
        ISender sender,
        CreateBrandCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/brands/{result.Id}", result);
    }

    private static async Task<IResult> UpdateBrand(
        ISender sender,
        Guid id,
        UpdateBrandRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new UpdateBrandCommand(id, request.Name, request.LogoUrl), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteBrand(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteBrandCommand(id), cancellationToken);
        return Results.NoContent();
    }
}

public sealed record UpdateBrandRequest(string Name, string? LogoUrl);
