using GreenDragonTrading.Application.Common.Options;
using GreenDragonTrading.Application.Interfaces;
using GreenDragonTrading.Domain.Interfaces;
using GreenDragonTrading.Infrastructure.BackgroundWorkers;
using GreenDragonTrading.Infrastructure.Persistence;
using GreenDragonTrading.Infrastructure.Persistence.Repositories;
using GreenDragonTrading.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
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
            string redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";

            // Register Unit of Work and Repositories here if needed
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IPostgreSqlGenericRepository<>), typeof(PostgreSqlGenericRepository<>));
            services.AddSingleton<ISsiStreamingService, SsiStreamingService>();
            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(redisConnectionString));
            services.AddScoped<IRedisService, RedisService>();
            services.AddSingleton<IMarketDataBroadcaster, MarketDataBroadcaster>();

            // Register JWT Service
            services.AddScoped<IJwtService, JwtService>();

            // Register Token Blacklist Service
            services.AddScoped<ITokenBlacklistService, TokenBlacklistService>();

            // Register Email Service
            services.AddScoped<IEmailService, EmailService>();

            // Register Cloudflare R2 File Storage Service (S3-compatible)
            var r2Options = configuration.GetSection(R2Options.SectionName).Get<R2Options>();
            if (r2Options != null)
            {
                var accessKeyId = configuration["AWS:Credentials:AccessKeyId"];
                var secretAccessKey = configuration["AWS:Credentials:SecretAccessKey"];
                
                if (!string.IsNullOrEmpty(accessKeyId) && !string.IsNullOrEmpty(secretAccessKey))
                {
                    // Configure S3 client for Cloudflare R2
                    var s3Config = new Amazon.S3.AmazonS3Config
                    {
                        ServiceURL = r2Options.Endpoint,
                        ForcePathStyle = true, // Required for R2
                        UseHttp = false
                    };

                    var credentials = new Amazon.Runtime.BasicAWSCredentials(accessKeyId, secretAccessKey);
                    var s3Client = new Amazon.S3.AmazonS3Client(credentials, s3Config);
                    
                    services.AddSingleton<Amazon.S3.IAmazonS3>(s3Client);
                }
                else
                {
                    throw new InvalidOperationException("AWS credentials are required for R2 file storage");
                }
            }
            else
            {
                services.AddDefaultAWSOptions(configuration.GetAWSOptions());
                services.AddAWSService<Amazon.S3.IAmazonS3>();
            }
            services.AddScoped<IFileStorageService, S3FileStorageService>();

            // Register Background Service for handling streaming events
            services.AddHostedService<SsiStreamingBackgroundService>();

            // Register Api options
            services.Configure<SsiApiOptionsV1>(configuration.GetSection(SsiApiOptionsV1.SectionName));
            services.Configure<SsiApiOptionsV2>(configuration.GetSection(SsiApiOptionsV2.SectionName));
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));
            services.Configure<R2Options>(configuration.GetSection(R2Options.SectionName));
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
                });
            }

            return services;
        }
    }

}
