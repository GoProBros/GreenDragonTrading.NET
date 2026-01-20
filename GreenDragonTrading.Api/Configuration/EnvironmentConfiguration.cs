namespace GreenDragonTrading.Api.Configuration;

/// <summary>
/// Helper class to build configuration from environment variables
/// This allows us to use .env file instead of hardcoding values in appsettings.json
/// </summary>
public static class EnvironmentConfiguration
{
    /// <summary>
    /// Configures the application to use environment variables from .env file
    /// </summary>
    public static void AddEnvironmentVariables(IConfigurationBuilder config)
    {
        // Build in-memory configuration from environment variables
        var envConfig = new Dictionary<string, string?>
        {
            // Database Configuration
            ["ConnectionStrings:GdtPostgreSqlConnection"] = BuildPostgresConnectionString(),
            ["ConnectionStrings:Redis"] = BuildRedisConnectionString(),

            // SSI API V1
            ["SsiApiV1:IBoardQuery"] = Environment.GetEnvironmentVariable("SSI_V1_IBOARD_QUERY"),
            ["SsiApiV1:IBoardApi"] = Environment.GetEnvironmentVariable("SSI_V1_IBOARD_API"),
            ["SsiApiV1:TimeoutSeconds"] = Environment.GetEnvironmentVariable("SSI_V1_TIMEOUT_SECONDS"),

            // SSI API V2
            ["SsiApiV2:FastConnectUrl"] = Environment.GetEnvironmentVariable("SSI_V2_FASTCONNECT_URL"),
            ["SsiApiV2:StreamURL"] = Environment.GetEnvironmentVariable("SSI_V2_STREAM_URL"),
            ["SsiApiV2:ConsumerID"] = Environment.GetEnvironmentVariable("SSI_V2_CONSUMER_ID"),
            ["SsiApiV2:ConsumerSecret"] = Environment.GetEnvironmentVariable("SSI_V2_CONSUMER_SECRET"),
            ["SsiApiV2:PublicKey"] = Environment.GetEnvironmentVariable("SSI_V2_PUBLIC_KEY"),
            ["SsiApiV2:PrivateKey"] = Environment.GetEnvironmentVariable("SSI_V2_PRIVATE_KEY"),
            ["SsiApiV2:TimeoutSeconds"] = Environment.GetEnvironmentVariable("SSI_V2_TIMEOUT_SECONDS"),

            // App Settings
            ["AppSettings:BaseUrl"] = Environment.GetEnvironmentVariable("APP_BASE_URL"),

            // JWT
            ["Jwt:Secret"] = Environment.GetEnvironmentVariable("JWT_SECRET"),
            ["Jwt:Issuer"] = Environment.GetEnvironmentVariable("JWT_ISSUER"),
            ["Jwt:Audience"] = Environment.GetEnvironmentVariable("JWT_AUDIENCE"),
            ["Jwt:AccessTokenExpirationMinutes"] = Environment.GetEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRATION_MINUTES"),
            ["Jwt:RefreshTokenExpirationDays"] = Environment.GetEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRATION_DAYS"),

            // Email
            ["Email:SmtpHost"] = Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST"),
            ["Email:SmtpPort"] = Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"),
            ["Email:SenderEmail"] = Environment.GetEnvironmentVariable("EMAIL_SENDER_EMAIL"),
            ["Email:SenderName"] = Environment.GetEnvironmentVariable("EMAIL_SENDER_NAME"),
            ["Email:Username"] = Environment.GetEnvironmentVariable("EMAIL_USERNAME"),
            ["Email:Password"] = Environment.GetEnvironmentVariable("EMAIL_PASSWORD"),
            ["Email:EnableSsl"] = Environment.GetEnvironmentVariable("EMAIL_ENABLE_SSL"),

            // PayOS
            ["PayOS:ClientId"] = Environment.GetEnvironmentVariable("PAYOS_CLIENT_ID"),
            ["PayOS:ApiKey"] = Environment.GetEnvironmentVariable("PAYOS_API_KEY"),
            ["PayOS:ChecksumKey"] = Environment.GetEnvironmentVariable("PAYOS_CHECKSUM_KEY"),
            ["PayOS:BaseUrl"] = Environment.GetEnvironmentVariable("PAYOS_BASE_URL") ?? "https://api-merchant.payos.vn",
            ["PayOS:ReturnUrl"] = Environment.GetEnvironmentVariable("PAYOS_RETURN_URL") ?? "",
            ["PayOS:CancelUrl"] = Environment.GetEnvironmentVariable("PAYOS_CANCEL_URL") ?? "",
            ["PayOS:ExpirationMinutes"] = Environment.GetEnvironmentVariable("PAYOS_EXPIRATION_MINUTES") ?? "30",
        };

        // Add CORS allowed origins (split by comma)
        var corsOrigins = Environment.GetEnvironmentVariable("CORS_ALLOWED_ORIGINS");
        if (!string.IsNullOrEmpty(corsOrigins))
        {
            var origins = corsOrigins.Split(',', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < origins.Length; i++)
            {
                envConfig[$"Cors:AllowedOrigins:{i}"] = origins[i].Trim();
            }
        }

        config.AddInMemoryCollection(envConfig!);
    }

    private static string? BuildPostgresConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("DATABASE_HOST");
        var database = Environment.GetEnvironmentVariable("DATABASE_NAME");
        var username = Environment.GetEnvironmentVariable("DATABASE_USERNAME");
        var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(database))
            return null;

        return $"Host={host};Database={database};Username={username};Password={password}";
    }

    private static string? BuildRedisConnectionString()
    {
        var host = Environment.GetEnvironmentVariable("REDIS_HOST");
        var port = Environment.GetEnvironmentVariable("REDIS_PORT");
        var password = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

        if (string.IsNullOrEmpty(host))
            return null;

        var connStr = $"{host}:{port ?? "6379"}";
        if (!string.IsNullOrEmpty(password))
        {
            connStr += $",password={password}";
        }

        return connStr;
    }
}
