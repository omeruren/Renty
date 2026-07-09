using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Renty.Server.Application.Common.Behaviors;

namespace Renty.Server.Application;

public static class ApplicationServiceRegistration
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof(ApplicationServiceRegistration).Assembly);

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
