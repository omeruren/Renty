using MediatR;
using Renty.Server.Application.Features.Profile.DTOs;

namespace Renty.Server.Application.Features.Profile.Queries.GetProfile;

public sealed record GetProfileQuery : IRequest<ProfileResponse>;
