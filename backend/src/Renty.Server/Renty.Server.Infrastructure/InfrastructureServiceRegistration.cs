using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Common.Interfaces;
using Renty.Server.Infrastructure.Configuration;
using Renty.Server.Infrastructure.Services;

namespace Renty.Server.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
