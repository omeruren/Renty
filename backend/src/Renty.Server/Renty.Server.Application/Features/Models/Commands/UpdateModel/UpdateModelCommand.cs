using MediatR;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Models.Commands.UpdateModel;

public sealed record UpdateModelCommand(
    Guid Id,
    string Name,
    VehicleCategory Category,
    int SeatCount,
    TransmissionType TransmissionType,
    FuelType FuelType) : IRequest<ModelResponse>;
