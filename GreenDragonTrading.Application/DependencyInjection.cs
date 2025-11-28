using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace GreenDragonTrading.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // Register FluentValidation
        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
