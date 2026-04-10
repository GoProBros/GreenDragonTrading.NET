using DotNetEnv;
using GreenDragonTrading.Api.Configuration;
using GreenDragonTrading.Api.Middlewares;
using GreenDragonTrading.Application;
using GreenDragonTrading.Infrastructure;
using GreenDragonTrading.Infrastructure.Hubs;
using Org.BouncyCastle.Asn1.Ocsp;
using Serilog;

// Load environment variables from .env file
Env.Load();

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting GreenDragonTrading .Net API");

    var builder = WebApplication.CreateBuilder(args);

    // Add environment variables from .env file to configuration
    EnvironmentConfiguration.AddEnvironmentVariables(builder.Configuration);

    // Configure Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        );

    // Add services to the container.
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();

    // Add CORS
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AppCorsPolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins ?? [])
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
    });


    // Add Exception Handler
    builder.Services.AddExceptionHandler<CustomExceptionHandler>();
    builder.Services.AddProblemDetails();

    // Add SignalR
    builder.Services.AddSignalR();

    // Configure file upload size limits (50MB)
    builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 52428800; // 50MB
    });

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        // Build metadata
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var assemblyVersion = assembly.GetName().Version?.ToString() ?? "1.0.0";
        var buildTime = File.GetLastWriteTimeUtc(assembly.Location);
        var utcPlus7 = TimeZoneInfo.FindSystemTimeZoneById(
            OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");
        var buildTimeLocal = TimeZoneInfo.ConvertTimeFromUtc(buildTime, utcPlus7);
        var buildTimeStr = $"{buildTime:yyyy-MM-dd HH:mm:ss} UTC  /  {buildTimeLocal:yyyy-MM-dd HH:mm:ss} UTC+7";

        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "GreenDragonTrading API",
            Version = $"v{assemblyVersion}",
            Description = $"Last build: **{buildTimeStr}**",
        });

        // Include XML comments from API project
        var apiXmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var apiXmlPath = Path.Combine(AppContext.BaseDirectory, apiXmlFile);
        options.IncludeXmlComments(apiXmlPath, includeControllerXmlComments: true);
        
        // Include XML comments from Application project (for DTOs, Commands, Queries)
        var applicationXmlFile = "GreenDragonTrading.Application.xml";
        var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
        if (File.Exists(applicationXmlPath))
        {
            options.IncludeXmlComments(applicationXmlPath);
        }

        // Configure Bearer Authentication for Swagger
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description = "Nhập JWT token theo format: Bearer {token}"
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    app.UseExceptionHandler(options => { });

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    // Use CORS
    app.UseCors("AppCorsPolicy");

    app.UseAuthentication();
    app.UseAuthorization();

    // Use Token Blacklist Middleware (buộc phải sau cái authen, authen rồi mới chạy dc cái middle ware n)
    app.UseTokenBlacklist();

    app.MapControllers();

    // Map SignalR Hub
    app.MapHub<MarketDataHub>("/hubs/marketdata");
    app.MapHub<NotificationHub>("/hubs/notifications");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}