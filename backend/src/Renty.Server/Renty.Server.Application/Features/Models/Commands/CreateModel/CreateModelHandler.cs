using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Models.Commands.CreateModel;

public sealed class CreateModelHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<CreateModelHandler> logger) : IRequestHandler<CreateModelCommand, ModelResponse>
{
    public async Task<ModelResponse> Handle(CreateModelCommand request, CancellationToken cancellationToken)
    {
        var brand = await context.Brands
            .FirstOrDefaultAsync(b => b.Id == request.BrandId, cancellationToken)
            ?? throw new NotFoundException(nameof(Brand), request.BrandId);

        var nameTaken = await context.Models
            .AnyAsync(m => m.Name == request.Name && m.BrandId == request.BrandId, cancellationToken);

        if (nameTaken)
            throw new BusinessRuleException("A model with this name already exists for this brand.");

        var model = new Model
        {
            Name = request.Name,
            Category = request.Category,
            SeatCount = request.SeatCount,
            TransmissionType = request.TransmissionType,
            FuelType = request.FuelType,
            BrandId = request.BrandId
        };

        context.Models.Add(model);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Model),
            EntityId = model.Id.ToString(),
            Action = AuditAction.Create,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Model {ModelId} created", model.Id);

        return new ModelResponse(
            model.Id, model.Name, model.Category, model.SeatCount, model.TransmissionType, model.FuelType,
            model.BrandId, brand.Name, model.CreatedAt);
    }
}
