# 📐 Coding Standards & Best Practices

> Tài liệu này định nghĩa các quy tắc và best practices mà tất cả developers trong team phải tuân theo khi làm việc với codebase GreenDragonTrading.

---

## 📋 Mục Lục

1. [Cấu Trúc Project](#1-cấu-trúc-project)
2. [Naming Conventions](#2-naming-conventions)
3. [Clean Architecture Rules](#3-clean-architecture-rules)
4. [CQRS & MediatR Standards](#4-cqrs--mediatr-standards)
5. [Entity & Database Standards](#5-entity--database-standards)
6. [Repository & Unit of Work](#6-repository--unit-of-work)
7. [Validation Standards](#7-validation-standards)
8. [Exception Handling](#8-exception-handling)
9. [API Response Standards](#9-api-response-standards)
10. [Controller Standards](#10-controller-standards)
11. [Dependency Injection](#11-dependency-injection)
12. [Logging Standards](#12-logging-standards)
13. [Security Standards](#13-security-standards)
14. [Code Style & Formatting](#14-code-style--formatting)
15. [Git Workflow](#15-git-workflow)
16. [Code Review Checklist](#16-code-review-checklist)

---

## 1. Cấu Trúc Project

### 1.1 Layer Dependencies

```
✅ ĐÚNG - Dependency hướng vào trong:

    API → Application → Domain
    Infrastructure → Application → Domain

❌ SAI - KHÔNG được dependency ngược:

    Domain → Application (SAI!)
    Application → Infrastructure (SAI!)
    Domain → Infrastructure (SAI!)
```

### 1.2 Folder Structure

```
📁 Domain/
├── 📁 Entities/          # Business entities
├── 📁 Enums/             # Business enums
├── 📁 Exceptions/        # Domain-specific exceptions
├── 📁 Interfaces/        # Repository interfaces, domain services
└── 📁 Events/            # Domain events (nếu có)

📁 Application/
├── 📁 Common/
│   ├── 📁 Behaviors/     # MediatR pipeline behaviors
│   └── 📁 Models/        # Shared models (ApiResponse, etc.)
├── 📁 DTOs/              # Data Transfer Objects theo feature
├── 📁 Interfaces/        # Interface cho external services
└── 📁 UseCases/          # CQRS Commands & Queries theo feature
    └── 📁 {Feature}/
        ├── 📁 Commands/
        │   └── 📁 {Action}/
        │       ├── {Action}Command.cs
        │       ├── {Action}CommandHandler.cs
        │       └── {Action}CommandValidator.cs
        └── 📁 Queries/
            └── 📁 {Action}/
                ├── {Action}Query.cs
                └── {Action}QueryHandler.cs

📁 Infrastructure/
├── 📁 Persistence/
│   ├── ApplicationDbContext.cs
│   ├── UnitOfWork.cs
│   ├── 📁 Configurations/ # Fluent API configurations
│   └── 📁 Repositories/   # Repository implementations
├── 📁 Services/           # External service implementations
└── 📁 BackgroundWorkers/  # Background jobs (nếu có)

📁 Api/
├── 📁 Controllers/        # API controllers
├── 📁 Middlewares/        # Custom middlewares
├── 📁 Filters/            # Action filters
└── Program.cs             # Entry point & DI configuration
```

---

## 2. Naming Conventions

### 2.1 General Rules

| Element | Convention | Example |
|---------|------------|---------|
| **Namespace** | PascalCase | `GreenDragonTrading.Application.UseCases` |
| **Class** | PascalCase (noun) | `UserRepository`, `JwtService` |
| **Interface** | I + PascalCase | `IUserRepository`, `IJwtService` |
| **Method** | PascalCase (verb) | `GetByIdAsync`, `CreateUser` |
| **Property** | PascalCase | `Username`, `CreatedAt` |
| **Private field** | _camelCase | `_unitOfWork`, `_logger` |
| **Local variable** | camelCase | `userId`, `accessToken` |
| **Constant** | PascalCase | `MaxRetryAttempts` |
| **Enum** | PascalCase (singular) | `UserRole` |
| **Enum value** | PascalCase | `UserRole.Admin` |

### 2.2 CQRS Naming

| Element | Pattern | Example |
|---------|---------|---------|
| **Command** | {Action}Command | `RegisterCommand`, `UpdateUserCommand` |
| **Command Handler** | {Action}CommandHandler | `RegisterCommandHandler` |
| **Command Validator** | {Action}CommandValidator | `RegisterCommandValidator` |
| **Query** | Get{Entity}Query | `GetUserByIdQuery`, `GetAllUsersQuery` |
| **Query Handler** | Get{Entity}QueryHandler | `GetUserByIdQueryHandler` |

### 2.3 File Naming

```
✅ ĐÚNG:
UserRepository.cs
RegisterCommand.cs
RegisterCommandHandler.cs
RegisterCommandValidator.cs

❌ SAI:
userRepository.cs
RegisterCmd.cs
RegisterHandler.cs
```

### 2.4 Database Naming (PostgreSQL)

| Element | Convention | Example |
|---------|------------|---------|
| **Table** | snake_case (plural) | `users`, `order_items` |
| **Column** | snake_case | `created_at`, `user_id` |
| **Primary Key** | `id` | `id` |
| **Foreign Key** | {table}_id | `user_id`, `order_id` |
| **Index** | ix_{table}_{column} | `ix_users_email` |
| **Unique Index** | uq_{table}_{column} | `uq_users_username` |

---

## 3. Clean Architecture Rules

### 3.1 Domain Layer Rules

```csharp
// ✅ ĐÚNG - Domain KHÔNG có external dependencies
namespace GreenDragonTrading.Domain.Entities;

using System.ComponentModel.DataAnnotations;  // ✅ OK - .NET built-in

public class User
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public string Username { get; set; } = string.Empty;
}

// ❌ SAI - KHÔNG dùng EF Core trong Domain
using Microsoft.EntityFrameworkCore;  // ❌ KHÔNG ĐƯỢC!

[Index(nameof(Username))]  // ❌ KHÔNG ĐƯỢC! Dùng Fluent API
public class User { }
```

### 3.2 Application Layer Rules

```csharp
// ✅ ĐÚNG - Chỉ định nghĩa Interface, KHÔNG implement
public interface IJwtService
{
    string GenerateAccessToken(User user);
}

// ❌ SAI - KHÔNG implement service trong Application
public class JwtService : IJwtService  // ❌ Phải ở Infrastructure!
{
    // ...
}
```

### 3.3 Infrastructure Layer Rules

```csharp
// ✅ ĐÚNG - Implement interfaces từ Domain/Application
public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    // ...
}

// ✅ ĐÚNG - Fluent API configuration thay vì Data Annotations
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>(entity =>
    {
        entity.HasIndex(e => e.Username).IsUnique();
        entity.HasIndex(e => e.Email).IsUnique();
    });
}
```

---

## 4. CQRS & MediatR Standards

### 4.1 Command Structure

```csharp
// ✅ ĐÚNG - Dùng record cho immutability
public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<AuthResponse>;

// ❌ SAI - Không dùng class với mutable properties
public class RegisterCommand : IRequest<AuthResponse>
{
    public string Username { get; set; }  // ❌ Mutable
}
```

### 4.2 Handler Rules

```csharp
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    // ✅ ĐÚNG - Inject dependencies qua constructor
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<AuthResponse> Handle(
        RegisterCommand request, 
        CancellationToken cancellationToken)
    {
        // ✅ ĐÚNG - Luôn truyền CancellationToken
        var user = await _unitOfWork.Users.GetByEmailAsync(
            request.Email, 
            cancellationToken);  // ✅

        // ✅ ĐÚNG - Throw custom exceptions
        if (user != null)
            throw new ConflictException(nameof(User), nameof(User.Email), request.Email);

        // ❌ SAI - Không dùng generic exceptions
        throw new Exception("Email already exists");  // ❌
    }
}
```

### 4.3 Query vs Command

```csharp
// ✅ COMMAND - Thay đổi state, có side effects
public record CreateOrderCommand(...) : IRequest<OrderDto>;
public record UpdateUserCommand(...) : IRequest<Unit>;
public record DeleteProductCommand(...) : IRequest<bool>;

// ✅ QUERY - Chỉ đọc data, KHÔNG có side effects
public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
public record GetAllOrdersQuery(int Page, int PageSize) : IRequest<PaginatedList<OrderDto>>;

// ❌ SAI - Query không nên thay đổi data
public class GetUserQueryHandler : IRequestHandler<GetUserQuery, UserDto>
{
    public async Task<UserDto> Handle(GetUserQuery request, ...)
    {
        user.LastAccessedAt = DateTime.UtcNow;  // ❌ Side effect trong Query!
        await _context.SaveChangesAsync();       // ❌ Không được!
    }
}
```

---

## 5. Entity & Database Standards

### 5.1 Entity Structure

```csharp
[Table("users")]  // ✅ Explicit table name
public class User
{
    // ✅ ĐÚNG - Primary key đầu tiên
    [Key]
    [Column("id")]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    // ✅ ĐÚNG - Required fields với MaxLength
    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    // ✅ ĐÚNG - Nullable fields
    [MaxLength(500)]
    [Column("bio")]
    public string? Bio { get; set; }

    // ✅ ĐÚNG - Audit fields ở cuối
    [Column("created_at")]
    public DateTime CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }
}
```

### 5.2 Relationships

```csharp
// ✅ ĐÚNG - Navigation properties
public class Order
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    // Foreign Key
    [Column("user_id")]
    public Guid UserId { get; set; }

    // Navigation property
    public User User { get; set; } = null!;

    // Collection navigation
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
```

### 5.3 Fluent API Configuration

```csharp
// ✅ ĐÚNG - Tách riêng configuration class
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Indexes
        builder.HasIndex(e => e.Username).IsUnique();
        builder.HasIndex(e => e.Email).IsUnique();

        // Relationships
        builder.HasMany(u => u.Orders)
               .WithOne(o => o.User)
               .HasForeignKey(o => o.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        // Default values
        builder.Property(e => e.CreatedAt)
               .HasDefaultValueSql("NOW()");
    }
}

// Đăng ký trong DbContext
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
```

---

## 6. Repository & Unit of Work

### 6.1 Generic Repository Pattern

```csharp
// ✅ ĐÚNG - Generic interface trong Domain
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Delete(T entity);
}

// ✅ ĐÚNG - Specific repository kế thừa generic
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
}
```

### 6.2 Unit of Work Pattern

```csharp
// ✅ ĐÚNG - UnitOfWork quản lý transactions
public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IOrderRepository Orders { get; }
    
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
}

// ✅ ĐÚNG - Sử dụng trong Handler
public async Task<OrderDto> Handle(CreateOrderCommand request, CancellationToken ct)
{
    await _unitOfWork.BeginTransactionAsync(ct);
    
    try
    {
        var order = new Order { ... };
        await _unitOfWork.Orders.AddAsync(order, ct);
        
        foreach (var item in request.Items)
        {
            // Update inventory
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, ct);
            product.Stock -= item.Quantity;
            _unitOfWork.Products.Update(product);
        }
        
        await _unitOfWork.SaveChangesAsync(ct);
        await _unitOfWork.CommitTransactionAsync(ct);
        
        return MapToDto(order);
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync(ct);
        throw;
    }
}
```

---

## 7. Validation Standards

### 7.1 FluentValidation Rules

```csharp
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        // ✅ ĐÚNG - Clear error messages
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$")
                .WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit")
            .Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character");
    }
}
```

### 7.2 Validation in Pipeline

```csharp
// ✅ ĐÚNG - ValidationBehavior tự động chạy trước Handler
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
            throw new ValidationException(failures);

        return await next();
    }
}
```

---

## 8. Exception Handling

### 8.1 Custom Exception Hierarchy

```csharp
// ✅ ĐÚNG - Base exception trong Domain
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
    protected DomainException(string message, Exception inner) : base(message, inner) { }
}

// Specific exceptions
public class NotFoundException : DomainException
{
    public string EntityName { get; }
    public object? Key { get; }

    public NotFoundException(string entityName, object? key = null)
        : base($"{entityName} was not found.")
    {
        EntityName = entityName;
        Key = key;
    }
}

public class ConflictException : DomainException
{
    public ConflictException(string entityName, string propertyName, string value)
        : base($"{entityName} with {propertyName} '{value}' already exists.")
    { }
}

public class UnauthorizedException : DomainException
{
    public UnauthorizedException(string message = "Unauthorized access.")
        : base(message) { }
}

public class BusinessRuleException : DomainException
{
    public string Code { get; }
    
    public BusinessRuleException(string code, string message) : base(message)
    {
        Code = code;
    }
}
```

### 8.2 Exception to HTTP Status Code Mapping

```csharp
// ✅ ĐÚNG - GlobalExceptionMiddleware
switch (exception)
{
    case ValidationException:          // → 400 Bad Request
    case BusinessRuleException:        // → 400 Bad Request
    case UnauthorizedException:        // → 401 Unauthorized
    case NotFoundException:            // → 404 Not Found
    case ConflictException:            // → 409 Conflict
    default:                           // → 500 Internal Server Error
}
```

### 8.3 When to Use Which Exception

| Situation | Exception | HTTP Status |
|-----------|-----------|-------------|
| Entity không tìm thấy | `NotFoundException` | 404 |
| Duplicate entry | `ConflictException` | 409 |
| Login thất bại | `UnauthorizedException` | 401 |
| Token hết hạn | `UnauthorizedException` | 401 |
| Vi phạm business rule | `BusinessRuleException` | 400 |
| Input không hợp lệ | `ValidationException` | 400 |
| Lỗi hệ thống | `Exception` (không catch) | 500 |

---

## 9. API Response Standards

### 9.1 Response Wrapper

```csharp
// ✅ ĐÚNG - Consistent response format
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
```

### 9.2 Response Examples

```json
// ✅ Success Response
{
    "success": true,
    "message": "User registered successfully",
    "data": {
        "userId": "abc-123",
        "username": "john",
        "email": "john@example.com"
    },
    "errors": null,
    "timestamp": "2025-11-29T10:30:00Z"
}

// ✅ Error Response - Validation
{
    "success": false,
    "message": "Validation failed",
    "data": null,
    "errors": [
        "Username is required",
        "Email is invalid"
    ],
    "timestamp": "2025-11-29T10:30:00Z"
}

// ✅ Error Response - Business Logic
{
    "success": false,
    "message": "User with Email 'john@example.com' already exists.",
    "data": null,
    "errors": null,
    "timestamp": "2025-11-29T10:30:00Z"
}
```

---

## 10. Controller Standards

### 10.1 Thin Controllers

```csharp
// ✅ ĐÚNG - Controller chỉ làm 3 việc:
// 1. Nhận request
// 2. Gửi đến MediatR
// 3. Trả response

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register(
        [FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Username, 
            request.Email, 
            request.Password);
            
        var result = await _mediator.Send(command);
        
        return Ok(ApiResponse<AuthResponse>.SuccessResponse(
            result, 
            "Registration successful"));
    }
}

// ❌ SAI - KHÔNG có business logic trong Controller
[HttpPost("register")]
public async Task<IActionResult> Register(RegisterRequest request)
{
    // ❌ Validation logic
    if (string.IsNullOrEmpty(request.Email))
        return BadRequest("Email is required");

    // ❌ Business logic
    var existingUser = await _userRepository.GetByEmailAsync(request.Email);
    if (existingUser != null)
        return Conflict("Email already exists");

    // ❌ Data access
    var user = new User { ... };
    await _context.Users.AddAsync(user);
    await _context.SaveChangesAsync();
    
    return Ok(user);
}
```

### 10.2 Route Conventions

```csharp
// ✅ ĐÚNG - RESTful conventions
[Route("api/[controller]")]  // → /api/users
public class UsersController : ControllerBase
{
    [HttpGet]                    // GET    /api/users
    [HttpGet("{id:guid}")]       // GET    /api/users/{id}
    [HttpPost]                   // POST   /api/users
    [HttpPut("{id:guid}")]       // PUT    /api/users/{id}
    [HttpPatch("{id:guid}")]     // PATCH  /api/users/{id}
    [HttpDelete("{id:guid}")]    // DELETE /api/users/{id}
    
    // Nested resources
    [HttpGet("{id:guid}/orders")]           // GET /api/users/{id}/orders
    [HttpPost("{id:guid}/orders")]          // POST /api/users/{id}/orders
}

// ✅ ĐÚNG - Action-based routes (khi không phải CRUD)
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    [HttpPost("register")]       // POST /api/auth/register
    [HttpPost("login")]          // POST /api/auth/login
    [HttpPost("logout")]         // POST /api/auth/logout
    [HttpPost("refresh-token")]  // POST /api/auth/refresh-token
}
```

---

## 11. Dependency Injection

### 11.1 Registration Pattern

```csharp
// ✅ ĐÚNG - Tách DI vào DependencyInjection.cs của mỗi layer

// Application/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}

// Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(...);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtService, JwtService>();
        
        return services;
    }
}

// Program.cs
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
```

### 11.2 Lifetime Rules

| Lifetime | Use Case | Example |
|----------|----------|---------|
| **Scoped** | Database context, Repositories | `AddScoped<IUserRepository, UserRepository>` |
| **Singleton** | Redis connection, Configuration | `AddSingleton<IConnectionMultiplexer>(...)` |
| **Transient** | Stateless services | `AddTransient<IEmailSender, EmailSender>` |

---

## 12. Logging Standards

### 12.1 Log Levels

| Level | When to Use | Example |
|-------|-------------|---------|
| **Verbose/Debug** | Development debugging | `_logger.LogDebug("Processing user {UserId}", userId)` |
| **Information** | Normal operations | `_logger.LogInformation("User registered: {Email}", email)` |
| **Warning** | Expected exceptions, business rule violations | `_logger.LogWarning("Login failed for {Email}", email)` |
| **Error** | Unexpected exceptions | `_logger.LogError(ex, "Failed to process order {OrderId}", orderId)` |
| **Fatal** | Application crash | `Log.Fatal(ex, "Application terminated unexpectedly")` |

### 12.2 Structured Logging

```csharp
// ✅ ĐÚNG - Structured logging với placeholders
_logger.LogInformation("User {UserId} logged in from {IpAddress}", 
    userId, 
    ipAddress);

_logger.LogWarning("Failed login attempt for {Email}. Attempt {AttemptCount} of {MaxAttempts}", 
    email, 
    attemptCount, 
    maxAttempts);

_logger.LogError(exception, 
    "Error processing payment for Order {OrderId}. Amount: {Amount}", 
    orderId, 
    amount);

// ❌ SAI - String interpolation
_logger.LogInformation($"User {userId} logged in");  // ❌ Không indexed
_logger.LogError($"Error: {exception.Message}");     // ❌ Mất context
```

### 12.3 Sensitive Data

```csharp
// ✅ ĐÚNG - KHÔNG log sensitive data
_logger.LogInformation("Password changed for user {UserId}", userId);

// ❌ SAI - KHÔNG BAO GIỜ log passwords, tokens, secrets
_logger.LogInformation("Login with password {Password}", password);  // ❌
_logger.LogDebug("JWT Token: {Token}", accessToken);                 // ❌
```

---

## 13. Security Standards

### 13.1 Password Requirements

```csharp
// ✅ Password rules
RuleFor(x => x.Password)
    .MinimumLength(8)
    .Matches("[A-Z]")      // Uppercase
    .Matches("[a-z]")      // Lowercase
    .Matches("[0-9]")      // Digit
    .Matches("[^a-zA-Z0-9]"); // Special character
```

### 13.2 JWT Configuration

```json
// ✅ appsettings.json - KHÔNG hard-code secrets
{
    "JwtSettings": {
        "SecretKey": "YOUR-SECRET-KEY-HERE-MINIMUM-32-CHARS",
        "Issuer": "GreenDragonTrading",
        "Audience": "GreenDragonTradingClients",
        "AccessTokenExpirationMinutes": 15,
        "RefreshTokenExpirationDays": 7
    }
}

// ✅ Production - Dùng Environment Variables hoặc Secret Manager
{
    "JwtSettings": {
        "SecretKey": "${JWT_SECRET_KEY}"  // Từ environment
    }
}
```

### 13.3 Authorization

```csharp
// ✅ ĐÚNG - Protect endpoints
[Authorize]                           // Yêu cầu authenticated
[HttpPost("logout")]
public async Task<IActionResult> Logout() { }

[Authorize(Roles = "Admin")]          // Yêu cầu role Admin
[HttpDelete("{id}")]
public async Task<IActionResult> Delete(Guid id) { }

[Authorize(Policy = "CanManageOrders")]  // Custom policy
[HttpPost("orders")]
public async Task<IActionResult> CreateOrder() { }

// ❌ SAI - Public endpoint truy cập data nhạy cảm
[HttpGet("users/{id}/profile")]       // ❌ Thiếu [Authorize]!
public async Task<IActionResult> GetProfile(Guid id) { }
```

---

## 14. Code Style & Formatting

### 14.1 File Organization

```csharp
// ✅ ĐÚNG - Thứ tự trong class
public class UserService
{
    // 1. Constants
    private const int MaxLoginAttempts = 5;

    // 2. Static fields
    private static readonly object _lock = new();

    // 3. Instance fields
    private readonly IUserRepository _userRepository;
    private readonly ILogger<UserService> _logger;

    // 4. Constructor
    public UserService(IUserRepository userRepository, ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    // 5. Public methods
    public async Task<User> GetUserAsync(Guid id) { }

    // 6. Private methods
    private void ValidateUser(User user) { }
}
```

### 14.2 Expression Body vs Block Body

```csharp
// ✅ Expression body - cho single-line methods
public string GetFullName() => $"{FirstName} {LastName}";

public bool IsExpired => ExpiresAt < DateTime.UtcNow;

// ✅ Block body - cho multi-line logic
public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
{
    var user = await _context.Users.FindAsync(id, ct);
    return user;
}
```

### 14.3 Null Handling

```csharp
// ✅ ĐÚNG - Null-conditional và null-coalescing
var username = user?.Username ?? "Anonymous";
var email = user?.Email;

// ✅ ĐÚNG - Pattern matching
if (user is null)
    throw new NotFoundException(nameof(User), id);

if (user is not null)
    return user;

// ✅ ĐÚNG - Required properties with null-forgiving
public User User { get; set; } = null!;  // EF Core sẽ populate
```

---

## 15. Git Workflow

### 15.1 Branch Naming

```
main              ← Production code
develop           ← Development integration
feature/*         ← New features
bugfix/*          ← Bug fixes
hotfix/*          ← Emergency fixes
release/*         ← Release preparation

Examples:
feature/user-authentication
feature/order-management
bugfix/login-validation
hotfix/security-patch
```

### 15.2 Commit Messages

```
<type>(<scope>): <subject>

<body>

<footer>
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation
- `style`: Formatting (no code change)
- `refactor`: Code refactoring
- `test`: Adding tests
- `chore`: Maintenance

**Examples:**
```
feat(auth): implement user registration

- Add RegisterCommand and Handler
- Add RegisterCommandValidator
- Add UserRepository.UsernameExistsAsync

Closes #123

---

fix(auth): resolve token expiration issue

The refresh token was not being validated correctly
against Redis stored value.

Fixes #456
```

### 15.3 Pull Request Checklist

- [ ] Code follows naming conventions
- [ ] No business logic in controllers
- [ ] All new code has validators
- [ ] Custom exceptions used appropriately
- [ ] Async methods have Async suffix
- [ ] CancellationToken passed through
- [ ] No hardcoded values
- [ ] Sensitive data not logged
- [ ] Unit tests added/updated

---

## 16. Code Review Checklist

### Architecture

- [ ] Layer dependencies are correct (Domain → ← Application → ← Infrastructure/API)
- [ ] No EF Core in Domain layer
- [ ] Business logic in Handlers, not Controllers
- [ ] Interfaces defined in correct layer

### CQRS & MediatR

- [ ] Commands use records (immutable)
- [ ] Handlers are stateless
- [ ] Each Handler has corresponding Validator
- [ ] CancellationToken used throughout

### Database

- [ ] Entity uses Data Annotations correctly
- [ ] Fluent API for indexes (not in Domain)
- [ ] Relationships properly configured
- [ ] Migrations reviewed

### Validation

- [ ] All inputs validated
- [ ] Clear error messages
- [ ] Appropriate validation rules

### Security

- [ ] Endpoints properly authorized
- [ ] No secrets in code
- [ ] Passwords hashed
- [ ] Sensitive data not logged

### Code Quality

- [ ] Naming conventions followed
- [ ] No magic numbers/strings
- [ ] Async/await used correctly
- [ ] Proper exception handling
- [ ] Logging at appropriate levels

---

## 📚 Quick Reference Card

```
┌─────────────────────────────────────────────────────────────────┐
│                    QUICK REFERENCE CARD                         │
├─────────────────────────────────────────────────────────────────┤
│ LAYER DEPENDENCIES:                                             │
│   Domain ← Application ← Infrastructure                         │
│   Domain ← Application ← API                                    │
│                                                                 │
│ CQRS STRUCTURE:                                                 │
│   UseCases/{Feature}/Commands/{Action}/                         │
│     - {Action}Command.cs (record : IRequest<T>)                 │
│     - {Action}CommandHandler.cs                                 │
│     - {Action}CommandValidator.cs                               │
│                                                                 │
│ EXCEPTIONS → HTTP:                                              │
│   ValidationException    → 400 Bad Request                      │
│   UnauthorizedException  → 401 Unauthorized                     │
│   NotFoundException      → 404 Not Found                        │
│   ConflictException      → 409 Conflict                         │
│   BusinessRuleException  → 400 Bad Request                      │
│   Exception (unexpected) → 500 Internal Server Error            │
│                                                                 │
│ NAMING:                                                         │
│   Class:    PascalCase (UserRepository)                         │
│   Method:   PascalCase + Async suffix (GetByIdAsync)            │
│   Field:    _camelCase (_userRepository)                        │
│   Variable: camelCase (userId)                                  │
│   DB Table: snake_case (order_items)                            │
│   DB Column: snake_case (created_at)                            │
│                                                                 │
│ DI LIFETIMES:                                                   │
│   Scoped:    DbContext, Repositories, UnitOfWork                │
│   Singleton: Redis, Configuration                               │
│   Transient: Stateless services                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

**Cập nhật lần cuối: November 2025**

**Maintainer: GreenDragonTrading Development Team**
