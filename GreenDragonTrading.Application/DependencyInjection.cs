using FluentValidation;
using GreenDragonTrading.Application.Layer.Behaviors;
using Microsoft.Extensions.DependencyInjection;

namespace GreenDragonTrading.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);


            services.AddHttpContextAccessor();

            return services;
        }
    }
}
