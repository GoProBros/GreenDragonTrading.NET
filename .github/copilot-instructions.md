# GreenDragonTrading.NET - AI Agent Instructions

## Project Overview
Real-time stock trading platform integrating with SSI (Securities Services Inc.) APIs. The system manages user authentication, fetches market data, handles user workspaces/subscriptions, streams live prices via SignalR, and caches data in Redis for performance.

## Architecture (Clean Architecture + DDD)

### Layer Dependencies
```
Api → Application → Domain
      ↓
Infrastructure → Application + Domain
```

**Never reference Infrastructure or Api from Application/Domain layers.**

### Core Components
- **Domain**: Entities (`Symbol`, `Sector`, `Exchange`, `User`, `Workspace`, `Subscription`, `UserSubscription`), Enums (`SymbolType`, `SymbolStatus`, `UserRole`, `CommonStatus`), Constants, Interfaces
- **Application**: Use cases (CQRS with MediatR), DTOs, FluentValidation with `ValidationBehavior<,>`, Service interfaces
- **Infrastructure**: DbContext (PostgreSQL/EF Core), Repositories, External services (SSI V1/V2/Streaming), Redis caching, SignalR Hub, Background workers (`SsiStreamingBackgroundService`)
- **Api**: ASP.NET Core controllers, JWT authentication, Swagger configuration, Serilog logging, Custom middlewares

### Key Patterns
1. **CQRS with MediatR**: All use cases are `IRequest<TResult>` with separate handlers
   - Commands: `GreenDragonTrading.Application/UseCases/*/Commands/` (e.g., `Auth/Commands/Login/`, `Workspace/Commands/CreateWorkspace/`)
   - Queries: `GreenDragonTrading.Application/UseCases/*/Queries/` (e.g., `Symbols/Queries/GetSymbols/`, `Auth/Queries/GetMe/`)
   - Example: `LoginCommand` + `LoginCommandHandler` in `Auth/Commands/Login/`

2. **Validation Pipeline**: FluentValidation with MediatR behavior
   ```csharp
   // In Application/DependencyInjection.cs
   cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
   services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
   ```
   - Validators in same folder as commands/queries
   - Auto-validated before handler execution

3. **Unit of Work + Repository**: Access DB via `IUnitOfWork` with scoped repositories
   ```csharp
   await _uow.Symbols.AddRangeAsync(symbols);
   await _uow.SaveChangesAsync();
   ```

4. **Dependency Injection Extensions**: Configure layers via `AddInfrastructure()` / `AddApplication()`
   - Located in `DependencyInjection.cs` in each project
   - Infrastructure registers: DbContext, UoW, Repositories, External services, HttpClients, JWT auth, Hosted services
   - Application registers: MediatR, FluentValidation, HttpContextAccessor

5. **Standard API Response**: Wrap all controller responses in `ApiResponse<T>`
   ```csharp
   // For simple responses
   return Ok(ApiResponse<TData>.Success(data, "Success message"));
   
   // For paginated responses (inherit PaginationQuery in query)
   var paginatedResponse = PaginatedResponse<TDto>.Create(items, totalCount, pageIndex, pageSize);
   return Ok(ApiResponse<PaginatedResponse<TDto>>.Success(paginatedResponse, "Success message"));
   
   // For non-generic responses
   return Ok(ApiResponse.Success("Success message"));
   
   // For error responses
   return BadRequest(ApiResponse.Failure("Error message", validationErrors));
   return BadRequest(ApiResponse<TData>.Failure("Error message", validationErrors));
   ```

## Exception Handling

### Custom Exception Handler
- Centralized exception handling via `CustomExceptionHandler` (IExceptionHandler)
- Registered in Program.cs: `builder.Services.AddExceptionHandler<CustomExceptionHandler>()`
- Automatically converts domain exceptions to appropriate HTTP status codes and `ApiResponse`

### Domain Exceptions (in `Domain/Exceptions/`)
- **`BusinessRuleException`** → 400 Bad Request: Business logic violations
- **`ValidationException`** → 400 Bad Request: FluentValidation failures with `IDictionary<string, string[]>` errors
- **`NotFoundException`** → 404 Not Found: Entity not found
- **`ConflictException`** → 409 Conflict: Duplicate/conflict errors
- **`AccessDeniedException`** → 403 Forbidden: Authorization failures
- **`UnauthenticatedException`** → 401 Unauthorized: Authentication failures
- **All others** → 500 Internal Server Error: Unexpected errors

**Usage in handlers:**
```csharp
throw new NotFoundException("User not found");
throw new ConflictException("Email already exists");
throw new BusinessRuleException("Cannot delete active workspace");
```


## Authentication & Authorization

### JWT Authentication
- Configured in `Infrastructure/DependencyInjection.cs` with JWT Bearer scheme
- Token validation: Issuer, Audience, Lifetime, IssuerSigningKey (symmetric)
- ClockSkew set to `TimeSpan.Zero` for strict expiration
- JWT options in `appsettings.json` → `JwtOptions` section

### Token Blacklist Middleware
- Custom middleware `TokenBlacklistMiddleware` checks if access tokens are blacklisted
- **MUST run after authentication**: `app.UseAuthentication()` → `app.UseTokenBlacklist()` → `app.UseAuthorization()`
- Extracts JTI from JWT claims and validates against `ITokenBlacklistService` (Redis-backed)
- Returns 401 if token is blacklisted

### Protected Endpoints
- Use `[Authorize]` attribute on controllers/actions requiring authentication
- Examples: `WorkspaceController.CreateWorkspace`, `AuthController.Logout`
- User claims accessible via `HttpContext.User`

## External Integrations

### SSI API Versions
- **V1** (`ISsiServiceV1`): Legacy REST API for symbols/sectors from `https://iboard-api.ssi.com.vn`
- **V2** (`ISsiServiceV2`): Modern REST API with RSA encryption, requires OAuth via `ISsiAuthService`
- **Streaming** (`ISsiStreamingService`): Real-time SignalR client connecting to `https://fc-datahub.ssi.com.vn`

### DNSE Integration
- **Service** (`IDnseService`): Financial report data provider
- **Mapper** (`IDnseDataMapper`): Maps DNSE API responses to domain entities
- Base URL: `Domain.Constants.DNSE.DnseConstants.API_BASE_URL`
- Registered as typed HttpClient with 30s timeout

### HttpClient Configuration
All external services use typed HttpClients registered in `Infrastructure/DependencyInjection.cs`:
- `ISsiServiceV1`: Timeout from options, `Accept: application/json`
- `ISsiServiceV2`: `Accept: application/x-www-form-urlencoded`
- `ISsiAuthService`: For OAuth token retrieval
- `IDnseService`: Base URL configured, 30s timeout

### File Storage (Cloudflare R2 / AWS S3)
- **Service** (`IFileStorageService`): Implemented by `S3FileStorageService`
- S3-compatible storage for Cloudflare R2 or AWS S3
- Configuration via `R2Options` and `AWS:Credentials` sections
- Requires `ServiceURL`, `AccessKeyId`, `SecretAccessKey`, `BucketName`
- Set `ForcePathStyle = true` for R2 compatibility

### Data Flow
1. **Import**: Controllers trigger MediatR commands → Services fetch from SSI → Bulk insert/update PostgreSQL
2. **Streaming**: `SsiStreamingBackgroundService` (hosted service) subscribes to SSI → Publishes to `IMarketDataBroadcaster` → SignalR broadcasts to clients
3. **Caching**: Market data stored in Redis with key pattern `MarketData:Symbol:{ticker}`


## Database & Persistence

### Connection Strings
- PostgreSQL: `GdtPostgreSqlConnection` in `appsettings.Development.json`
- Redis: `Redis` connection string (supports password)

### Migrations
```powershell
# Add migration
dotnet ef migrations add MigrationName --project GreenDragonTrading.Infrastructure --startup-project GreenDragonTrading.Api

# Apply migration
dotnet ef database update --project GreenDragonTrading.Infrastructure --startup-project GreenDragonTrading.Api
```

### Entity Conventions
- Table names: lowercase with underscores (`symbols`, `sectors`)
- Primary keys: `[Key]` attribute, typed as `string` (ticker), `Guid`, or `int`
- Columns: `[Column("column_name")]` with explicit `TypeName` for PostgreSQL types
- Navigation: Use `ExchangeCode`/`SectorId` foreign keys (explicit relationships in `OnModelCreating`)

## Development Workflow

### Running the Application
```powershell
# From solution root
dotnet run --project GreenDragonTrading.Api

# Access Swagger UI
# https://localhost:7148/swagger (default HTTPS port)
```

### Quick Start Script
Use `run-server-and-client.ps1` to launch both API and SignalR test client simultaneously.

### Project Structure
- `.sln` file at root: Reference this for builds
- `appsettings.Development.json`: Local config (NOT committed with real credentials)
- `Logs/`: Serilog file outputs (rolling daily, 7-day retention)
- `GreenDragonTrading.SignalRClient/`: Standalone console app for testing SignalR hub

### Key Configuration Sections
- `SsiApiV1`: IBoard API endpoints
- `SsiApiV2`: FastConnect credentials (ConsumerID, ConsumerSecret, RSA keys)
- `JwtOptions`: JWT secret, issuer, audience, token lifetimes
- `EmailOptions`: SMTP configuration for email verification/password reset
- `R2Options` / `AWS:Credentials`: Cloudflare R2 or AWS S3 storage configuration
- `Cors:AllowedOrigins`: Array of allowed CORS origins
- `Serilog`: Console + File sinks with structured logging

### Environment Configuration
- Uses `DotNetEnv` package to load `.env` file at startup
- Call `Env.Load()` before building the app
- Environment variables override `appsettings.json` via `EnvironmentConfiguration.AddEnvironmentVariables()`
- See `.env.example` for required configuration keys
- **Never commit `.env` files** - they contain secrets

### Middleware Order (Critical)
```csharp
// In Program.cs - MUST follow this exact order
app.UseExceptionHandler(options => { });        // 1. Exception handling first
app.UseCors("AppCorsPolicy");                   // 2. CORS before auth
app.UseAuthentication();                        // 3. Authenticate user
app.UseAuthorization();                         // 4. Authorize user (BEFORE TokenBlacklist!)
app.UseTokenBlacklist();                        // 5. Check token blacklist (requires authenticated user)
app.MapControllers();
app.MapHub<MarketDataHub>("/hubs/marketdata");
```

## Real-Time Features

### SignalR Hub (`/hubs/marketdata`)
- **Methods**: `SubscribeToSymbols(string[])`, `UnsubscribeFromSymbols(string[])`, `GetCurrentPrices(string[])`
- Clients receive: `ReceiveMarketData` (single), `ReceiveBatchMarketData` (multiple)
- Uses Redis for current price lookups

### Background Service
`SsiStreamingBackgroundService` runs on startup:
1. Authenticates with SSI V2
2. Subscribes to market data channels
3. Processes SSI streaming responses (X, Z channels)
4. Broadcasts via `IMarketDataBroadcaster` (SignalR hub wrapper)

## Response Models

### ApiResponse<T>
Standard response wrapper for all API endpoints. Located in `GreenDragonTrading.Application/Common/Models/ApiResponse.cs`.

**Properties:**
- `IsSuccess` (bool): Indicates if the request was successful
- `Message` (string): Human-readable message
- `Data` (T, optional): Response payload (generic version only)
- `ValidationErrors` (IDictionary<string, string[]>, optional): Validation errors by field
- `ResponseTime` (DateTime): Timestamp of the response

**Factory Methods:**
```csharp
// Non-generic version
ApiResponse.Success(string message = "Thành công")
ApiResponse.Failure(string message, IDictionary<string, string[]> validationErrors)
ApiResponse.Failure(string message)

// Generic version
ApiResponse<T>.Success(T data, string message = "Thành công")
ApiResponse<T>.Failure(string message, IDictionary<string, string[]> validationErrors)
ApiResponse<T>.Failure(string message)
```

### PaginatedResponse<T>
Wrapper for paginated data responses. Located in `GreenDragonTrading.Application/Common/Models/PaginatedResponse.cs`.

**Properties:**
- `Items` (IReadOnlyCollection<T>): The data items for current page
- `PageIndex` (int): Current page number (1-based)
- `TotalPages` (int): Total number of pages
- `TotalCount` (int): Total number of items across all pages
- `HasPreviousPage` (bool): Computed property
- `HasNextPage` (bool): Computed property

**Factory Method:**
```csharp
PaginatedResponse<T>.Create(List<T> items, int count, int pageIndex, int pageSize)
```

**Usage Pattern:**
```csharp
// In handler
var (data, totalCount) = await _repository.GetPaginatedAsync(pageIndex, pageSize, cancellationToken);
var items = data.Select(x => MapToDto(x)).ToList();
var paginatedResponse = PaginatedResponse<TDto>.Create(items, totalCount, pageIndex, pageSize);
return ApiResponse<PaginatedResponse<TDto>>.Success(paginatedResponse, "Success message");

// In controller
var result = await _mediator.Send(query, cancellationToken);
return Ok(result); // ApiResponse is already wrapped by handler
```

## Code Conventions

### Naming
- **Commands/Queries**: Verb-based, e.g., `ImportSymbolsFromSsiCommandV1`, `GetSymbolsQuery`
- **Results**: Suffix with `Result`, e.g., `ImportSymbolsFromSsiV1Result`
- **DTOs**: Suffix with `Dto`, e.g., `SsiSymbolDto`, `MarketDataDto`

### Nullability
- Nullable reference types enabled (`<Nullable>enable</Nullable>`)
- Use `null!` only for required properties set by ORM/DI

### Async Patterns
- All I/O operations use `async/await`
- Accept `CancellationToken cancellationToken = default` in public methods
- Pass tokens to EF Core methods: `ToListAsync(cancellationToken)`

### Logging
- Use ILogger via constructor injection
- Structured logging: `_logger.LogInformation("Message {Property}", value);`
- Log levels: Debug (SSI responses), Information (operations), Warning (errors), Error (exceptions)

## Testing & Debugging

### No Tests Currently
- Test project folder exists but empty (`tests/` in solution)
- When adding tests, reference `GreenDragonTrading.Application` for use case testing

### Common Issues
- **SSI Auth Failures**: Check ConsumerID/Secret in `appsettings.Development.json`, verify RSA keys
- **Redis Connection**: Ensure Redis is running on `localhost:6379` or configured port
- **PostgreSQL**: Verify connection string and database exists (`greendragontrading_v1`)

## Important Constants

### Exchange Codes (Domain)
- HSX: `ExchangeConstant.EXCHANGE_HSX` → `"hsx"`
- HNX: `ExchangeConstant.EXCHANGE_HNX` → `"hnx"`
- UPCOM: `ExchangeConstant.EXCHANGE_UPCOM` → `"upcom"`

### SSI Constants
- Located in `GreenDragonTrading.Domain/Constants/SSI/`
- Map SSI codes to domain enums (e.g., `SsiConstantsV1.SSI_EXCHANGE_HSX` → domain `ExchangeCode`)

## When Adding Features

1. **New Entity**: Add to `Domain/Entities/`, create migration, update `DbContext.OnModelCreating()`
2. **New Use Case**: Create Command/Query in `Application/UseCases/`, implement handler, register automatically via MediatR assembly scanning
3. **New External Service**: Define interface in `Application/Interfaces/`, implement in `Infrastructure/Services/`, register in `DependencyInjection.AddInfrastructure()`
4. **New Controller**: Inject `IMediator`, use `ApiResponse<T>`, add XML comments for Swagger

## API Documentation
- Swagger auto-generated from XML comments (`GenerateDocumentationFile` enabled)
- Use `/// <summary>` tags on controllers and actions
- Include `/// <param>` and `/// <returns>` for clarity
