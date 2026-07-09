using MediatR;
using Renty.Server.Application.Features.Models.DTOs;
using Renty.Server.Domain.Enums;

namespace Renty.Server.Application.Features.Models.Commands.CreateModel;

public sealed record CreateModelCommand(
    string Name,
    VehicleCategory Category,
    int SeatCount,
    TransmissionType TransmissionType,
    FuelType FuelType,
    Guid BrandId) : IRequest<ModelResponse>;
