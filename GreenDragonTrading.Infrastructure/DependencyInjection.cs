using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.BackgroundWorkers;
using GreenDragonTrading.Infrastructure.Persistence;
using GreenDragonTrading.Infrastructure.Persistence.Repositories;
using GreenDragonTrading.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace GreenDragonTrading.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddMemoryCache();

            // Register DbContext
            services.AddDbContext<GdtPostgreSqlDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("GdtPostgreSqlConnection"),
                    b => b.MigrationsAssembly(typeof(GdtPostgreSqlDbContext).Assembly.FullName)));
            string redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            // Register Unit of Work and Repositories here if needed
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IPostgreSqlGenericRepository<>), typeof(PostgreSqlGenericRepository<>));
            services.AddSingleton<ISsiStreamingService, SsiStreamingService>();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddScoped<IRedisService, RedisService>();
            services.AddSingleton<IMarketDataBroadcaster, MarketDataBroadcaster>();

            // Register Background Service for handling streaming events
            services.AddHostedService<SsiStreamingBackgroundService>();

            // Register Api options
            services.Configure<SsiApiOptionsV1>(configuration.GetSection(SsiApiOptionsV1.SectionName));
            services.Configure<SsiApiOptionsV2>(configuration.GetSection(SsiApiOptionsV2.SectionName));

            // Register HttpClient
            services.AddHttpClient<ISsiServiceV1, SsiServiceV1>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SsiApiOptionsV1>>().Value;

                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "GDT/1.0");
            });
            services.AddHttpClient<ISsiServiceV2, SsiServiceV2>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SsiApiOptionsV2>>().Value;

                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Add("User-Agent", "GDT/1.0");
            });
            services.AddHttpClient<ISsiAuthService, SsiAuthService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SsiApiOptionsV2>>().Value;

                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "GDT/1.0");
            });

            return services;
        }
    }

}
