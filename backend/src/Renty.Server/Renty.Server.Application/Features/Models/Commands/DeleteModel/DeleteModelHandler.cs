using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Renty.Server.Application.Common.Exceptions;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Domain.Entities;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Models.Commands.DeleteModel;

public sealed class DeleteModelHandler(
    IApplicationDbContext context,
    ICurrentUserService currentUserService,
    ILogger<DeleteModelHandler> logger) : IRequestHandler<DeleteModelCommand>
{
    public async Task Handle(DeleteModelCommand request, CancellationToken cancellationToken)
    {
        var model = await context.Models
            .FirstOrDefaultAsync(m => m.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Model), request.Id);

        var hasCars = await context.Cars.AnyAsync(c => c.ModelId == request.Id, cancellationToken);

        if (hasCars)
            throw new BusinessRuleException("Cannot delete a model with existing cars.");

        context.Models.Remove(model);

        context.AuditLogs.Add(new AuditLog
        {
            EntityName = nameof(Model),
            EntityId = model.Id.ToString(),
            Action = AuditAction.Delete,
            UserId = currentUserService.UserId,
            Timestamp = DateTime.UtcNow,
            IpAddress = currentUserService.IpAddress
        });

        await context.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Model {ModelId} deleted", model.Id);
    }
}
