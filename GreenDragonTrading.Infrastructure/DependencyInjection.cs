using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.BackgroundWorkers;
using GreenDragonTrading.Infrastructure.Hubs;
using GreenDragonTrading.Infrastructure.Persistence;
using GreenDragonTrading.Infrastructure.Persistence.Repositories;
using GreenDragonTrading.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.SignalR;
using StackExchange.Redis;
using System.Text;

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
            
            // Register OhlcvTimescaleDbContext
            services.AddDbContext<OhlcvTimescaleDbContext>(options =>
                options.UseNpgsql(
                    configuration.GetConnectionString("OhlcvTimescaleDbConnection"),
                    b => b.MigrationsAssembly(typeof(OhlcvTimescaleDbContext).Assembly.FullName)));
            
            string redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            // Register Unit of Work and Repositories here if needed
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IPostgreSqlGenericRepository<>), typeof(PostgreSqlGenericRepository<>));
            services.AddScoped<IOhlcvUnitOfWork, OhlcvUnitOfWork>();
            services.AddScoped<IOhlcvRepository, OhlcvRepository>();
            services.AddSingleton<ISsiStreamingService, SsiStreamingService>();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddScoped<IRedisService, RedisService>();
            services.AddSingleton<IMarketDataBroadcaster, MarketDataBroadcaster>();
            services.AddSingleton<IUserIdProvider, MarketDataUserIdProvider>();
            services.AddScoped<IHeatmapService, HeatmapService>();

            // Register JWT Service
            services.AddScoped<IJwtService, JwtService>();

            // Register Current User Service
            services.AddScoped<ICurrentUserService, CurrentUserService>();

            // Register Token Blacklist Service
            services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

            // Register Log Reader Service
            services.AddScoped<ILogReaderService, LogReaderService>();
            services.Configure<LogOptions>(configuration.GetSection(LogOptions.SectionName));

            // Register Email Service
            services.AddScoped<IEmailService, EmailService>();

            // Register Local File Storage Service
            services.AddScoped<ILocalFileStorageService, LocalFileStorageService>();

            // Register Workspace Duplication Service
            services.AddScoped<IWorkspaceDuplicationService, WorkspaceDuplicationService>();

            // Register Google Auth Service
            services.AddScoped<IGoogleAuthService, GoogleAuthService>();

            // Register Api options
            // Register Api options (must be configured before registering Background Services)
            services.Configure<SsiApiOptionsV1>(configuration.GetSection(SsiApiOptionsV1.SectionName));
            services.Configure<SsiApiOptionsV2>(configuration.GetSection(SsiApiOptionsV2.SectionName));
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            services.Configure<PayOSOptions>(configuration.GetSection(PayOSOptions.SectionName));

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

                client.BaseAddress = new Uri(options.FastConnectUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/x-www-form-urlencoded");
                client.DefaultRequestHeaders.Add("User-Agent", "GDT/1.0");
            });
            services.AddHttpClient<ISsiAuthService, SsiAuthService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<SsiApiOptionsV2>>().Value;

                // Add null check and logging
                if (string.IsNullOrEmpty(options.FastConnectUrl))
                {
                    throw new InvalidOperationException(
                        "SsiApiV2:FastConnectUrl is not configured. " +
                        $"ConsumerID: {options.ConsumerID ?? "null"}, " +
                        $"TimeoutSeconds: {options.TimeoutSeconds}");
                }

                client.BaseAddress = new Uri(options.FastConnectUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "GDT/1.0");
            });

            // Register DNSE Service
            services.AddHttpClient<IDnseService, DnseService>((sp, client) =>
            {
                client.BaseAddress = new Uri(Domain.Constants.DNSE.DnseConstants.API_BASE_URL);
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "GDT/1.0");
            });
            
            // Register DNSE Data Mapper
            services.AddScoped<IDnseDataMapper, DnseDataMapper>();

            // Register PayOS Service
            services.AddScoped<IPayOSService, PayOSService>();
            services.AddScoped<IPaymentService, PaymentService>();

            // Register Momo Service
            services.Configure<MomoOptions>(configuration.GetSection(MomoOptions.SectionName));
            services.AddHttpClient<IMomoService, MomoService>();

            // Register AI Chat Service
            services.Configure<AiEngineOptions>(configuration.GetSection(AiEngineOptions.SectionName));
            services.AddHttpClient<IAiChatService, AiChatService>((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<AiEngineOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
                client.DefaultRequestHeaders.Add("User-Agent", "GDT/1.0");
            });

            // Add JWT Authentication
            var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();
            if (jwtOptions != null)
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Secret)),
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;

                            if (!string.IsNullOrEmpty(accessToken)
                                && path.StartsWithSegments("/hubs/marketdata"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });
            }

            // Register Background Services (after all dependencies are configured)
            services.AddHostedService<SsiStreamingBackgroundService>();
            services.AddHostedService<PriceAdjustmentCheckService>();
            services.AddHostedService<IndicatorCalculationBackgroundService>();

            return services;
        }
    }

}
