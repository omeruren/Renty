using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Models.Commands.UpdateModel;

public sealed class UpdateModelHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<UpdateModelHandler> logger) : IRequestHandler<UpdateModelCommand, ModelResponse>
{
    public async Task<ModelResponse> Handle(UpdateModelCommand request, CancellationToken cancellationToken)
    {
        var model = await context.Models
            .Include(m => m.Brand)
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Model), request.Id);

        var nameTaken = await context.Models
            .AnyAsync(
                m => m.Name == request.Name && m.BrandId == model.BrandId && m.Id != request.Id,
                cancellationToken);

        if (nameTaken)
            throw new BusinessRuleException("A model with this name already exists for this brand.");

        model.Name = request.Name;
        model.Category = request.Category;
        model.SeatCount = request.SeatCount;
        model.TransmissionType = request.TransmissionType;
        model.FuelType = request.FuelType;

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Model),
            EntityId = model.Id.ToString(),
            Action = AuditAction.Update,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Model {ModelId} updated", model.Id);

        return new ModelResponse(
            model.Id, model.Name, model.Category, model.SeatCount, model.TransmissionType, model.FuelType,
            model.BrandId, model.Brand.Name, model.CreatedAt);
    }
}
