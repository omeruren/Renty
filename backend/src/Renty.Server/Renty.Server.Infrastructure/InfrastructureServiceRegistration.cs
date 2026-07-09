using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Infrastructure.Services;

namespace Renty.Server.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
