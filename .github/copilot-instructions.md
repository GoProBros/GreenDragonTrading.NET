# GreenDragonTrading.NET - AI Agent Instructions

## Project Overview
Real-time stock trading platform integrating with SSI (Securities Services Inc.) APIs. The system fetches market data, manages symbols/sectors, streams live prices via SignalR, and caches data in Redis for performance.

## Architecture (Clean Architecture + DDD)

### Layer Dependencies
```
Api → Application → Domain
      ↓
Infrastructure → Application + Domain
```

**Never reference Infrastructure or Api from Application/Domain layers.**

### Core Components
- **Domain**: Entities (`Symbol`, `Sector`, `Exchange`), Enums (`SymbolType`, `SymbolStatus`), Constants, Interfaces
- **Application**: Use cases (CQRS with MediatR), DTOs, FluentValidation, Service interfaces
- **Infrastructure**: DbContext (PostgreSQL/EF Core), Repositories, External services (SSI V1/V2/Streaming), Redis caching, SignalR Hub, Background workers
- **Api**: ASP.NET Core controllers, Swagger configuration, Serilog logging

### Key Patterns
1. **CQRS with MediatR**: All use cases are `IRequest<TResult>` with separate handlers
   - Commands: `GreenDragonTrading.Application/UseCases/*/Commands/`
   - Queries: `GreenDragonTrading.Application/UseCases/*/Queries/`
   - Example: `ImportSymbolsFromSsiCommandV1` + `ImportSymbolsFromSsiCommandHandlerV1`

2. **Unit of Work + Repository**: Access DB via `IUnitOfWork` with scoped repositories
   ```csharp
   await _uow.Symbols.AddRangeAsync(symbols);
   await _uow.SaveChangesAsync();
   ```

3. **Dependency Injection Extensions**: Configure layers via `AddInfrastructure()` / `AddApplication()`
   - Located in `DependencyInjection.cs` in each project

4. **Standard API Response**: Wrap all controller responses in `ApiResponse<T>`
   ```csharp
   // For simple responses
   return Ok(ApiResponse<TData>.Success(data, "Success message"));
   
   // For paginated responses
   var paginatedResponse = PaginatedResponse<TData>.Create(items, totalCount, pageIndex, pageSize);
   return Ok(ApiResponse<PaginatedResponse<TData>>.Success(paginatedResponse, "Success message"));
   
   // For non-generic responses
   return Ok(ApiResponse.Success("Success message"));
   
   // For error responses
   return BadRequest(ApiResponse.Failure("Error message", "Error detail"));
   return BadRequest(ApiResponse<TData>.Failure("Error message", errorList));
   ```

## External Integrations

### SSI API Versions
- **V1** (`ISsiServiceV1`): Legacy REST API for symbols/sectors from `https://iboard-api.ssi.com.vn`
- **V2** (`ISsiServiceV2`): Modern REST API with RSA encryption, requires OAuth via `ISsiAuthService`
- **Streaming** (`ISsiStreamingService`): Real-time SignalR client connecting to `https://fc-datahub.ssi.com.vn`

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

### Project Structure
- `.sln` file at root: Reference this for builds
- `appsettings.Development.json`: Local config (NOT committed with real credentials)
- `Logs/`: Serilog file outputs (rolling daily, 7-day retention)

### Key Configuration Sections
- `SsiApiV1`: IBoard API endpoints
- `SsiApiV2`: FastConnect credentials (ConsumerID, ConsumerSecret, RSA keys)
- `Serilog`: Console + File sinks with structured logging

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
- `Errors` (List<string>, optional): List of error messages
- `ResponseTime` (DateTime): Timestamp of the response

**Factory Methods:**
```csharp
// Non-generic version
ApiResponse.Success(string message = "Thành công")
ApiResponse.Failure(string message, List<string> errors)
ApiResponse.Failure(string message, string? error = null)

// Generic version
ApiResponse<T>.Success(T data, string message = "Thành công")
ApiResponse<T>.Failure(string message, List<string> errors)
ApiResponse<T>.Failure(string message, string? error = null)
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
