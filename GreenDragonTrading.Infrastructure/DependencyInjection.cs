using GreenDragonTrading.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GreenDragonTrading.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<GdtPostgreSqlDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("GdtPostgreSqlConnection"),
                    b => b.MigrationsAssembly(typeof(GdtPostgreSqlDbContext).Assembly.FullName)));

            return services;
        }
    }

}
