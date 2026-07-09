using MediatR;
using Renty.Server.Application.Features.Profile.DTOs;

namespace Renty.Server.Application.Features.Profile.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    string FirstName,
    string LastName,
    string? PhoneNumber,
    DateOnly? DateOfBirth) : IRequest<ProfileResponse>;
