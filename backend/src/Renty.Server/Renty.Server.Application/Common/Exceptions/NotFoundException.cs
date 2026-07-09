namespace Renty.Server.Application.Common.Exceptions;

public sealed class NotFoundException(string entityName, object key)
    : Exception($"{entityName} with ID '{key}' was not found.");
