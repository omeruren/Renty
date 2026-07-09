namespace Renty.Server.Application.Common.Exceptions;

public sealed class ForbiddenException(string message) : Exception(message);
