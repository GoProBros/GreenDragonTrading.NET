using GreenDragonTrading.Api.Configuration;
using GreenDragonTrading.Api.Middlewares;
using GreenDragonTrading.Application;
using GreenDragonTrading.Infrastructure;
using GreenDragonTrading.Infrastructure.Hubs;
using Serilog;
using DotNetEnv;

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

    // Inject Google Drive credentials from environment variable if available
    var googleDriveCredentials = Environment.GetEnvironmentVariable("GOOGLE_DRIVE_CREDENTIALS");
    if (!string.IsNullOrEmpty(googleDriveCredentials))
    {
        builder.Configuration["GoogleDrive:JsonCredentials"] = googleDriveCredentials;
        Log.Information("Google Drive credentials loaded from environment variable");
    }

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

    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
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