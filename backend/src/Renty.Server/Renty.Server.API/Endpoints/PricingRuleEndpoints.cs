using MediatR;
using Renty.Server.Application.Common.Models;
using Renty.Server.Application.Features.PricingRules.Commands.CreatePricingRule;
using Renty.Server.Application.Features.PricingRules.Commands.DeletePricingRule;
using Renty.Server.Application.Features.PricingRules.Commands.UpdatePricingRule;
using Renty.Server.Application.Features.PricingRules.DTOs;
using Renty.Server.Application.Features.PricingRules.Queries.GetAllPricingRules;
using Renty.Server.Application.Features.PricingRules.Queries.GetPricingRuleById;
using Renty.Server.Domain.Enums;

namespace Renty.Server.API.Endpoints;

public static class PricingRuleEndpoints
{
    public static void MapPricingRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/pricing-rules")
            .WithTags("Pricing Rules")
            .RequireAuthorization("CanManagePricing");

        group.MapGet("/", GetAllPricingRules)
            .WithName("GetAllPricingRules")
            .Produces<PagedResponse<PricingRuleListResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{id:guid}", GetPricingRuleById)
            .WithName("GetPricingRuleById")
            .Produces<PricingRuleResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", CreatePricingRule)
            .WithName("CreatePricingRule")
            .RequireAuthorization("AdminOnly")
            .Produces<PricingRuleResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdatePricingRule)
            .WithName("UpdatePricingRule")
            .RequireAuthorization("AdminOnly")
            .Produces<PricingRuleResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}", DeletePricingRule)
            .WithName("DeletePricingRule")
            .RequireAuthorization("AdminOnly")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetAllPricingRules(
        ISender sender,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 10,
        bool? isActive = null)
    {
        var result = await sender.Send(new GetAllPricingRulesQuery(page, pageSize, isActive), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetPricingRuleById(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetPricingRuleByIdQuery(id), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> CreatePricingRule(
        ISender sender,
        CreatePricingRuleCommand command,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/pricing-rules/{result.Id}", result);
    }

    private static async Task<IResult> UpdatePricingRule(
        ISender sender,
        Guid id,
        UpdatePricingRuleRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdatePricingRuleCommand(
            id, request.Name, request.RuleType, request.Multiplier, request.StartDate, request.EndDate,
            request.VehicleCategory, request.IsActive, request.Priority);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeletePricingRule(ISender sender, Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePricingRuleCommand(id), cancellationToken);
        return Results.NoContent();
    }
}

public sealed record UpdatePricingRuleRequest(
    string Name,
    PricingRuleType RuleType,
    decimal Multiplier,
    DateTime? StartDate,
    DateTime? EndDate,
    VehicleCategory? VehicleCategory,
    bool IsActive,
    int Priority);
