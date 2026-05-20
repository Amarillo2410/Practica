using System;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services)
    {
        var applicationAssembly = typeof(ApplicationAssemblyReference).Assembly;

        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(applicationAssembly));

        services.AddValidatorsFromAssembly(applicationAssembly);

        return services;
    }
}
