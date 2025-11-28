# 📚 Hướng Dẫn Đọc Code - GreenDragonTrading API

> Tài liệu này dành cho người mới bắt đầu, chưa từng làm việc với Clean Architecture, CQRS, MediatR hoặc Entity Framework Code-First.

---

## 📋 Mục Lục

1. [Tổng Quan Kiến Trúc](#1-tổng-quan-kiến-trúc)
2. [Các Thư Viện Chính](#2-các-thư-viện-chính)
3. [Cấu Trúc Thư Mục](#3-cấu-trúc-thư-mục)
4. [Flow Dữ Liệu - Ví Dụ Đăng Ký Tài Khoản](#4-flow-dữ-liệu---ví-dụ-đăng-ký-tài-khoản)
5. [Giải Thích Chi Tiết Từng Layer](#5-giải-thích-chi-tiết-từng-layer)
6. [Entity Framework Code-First](#6-entity-framework-code-first)
7. [CQRS + MediatR Pattern](#7-cqrs--mediatr-pattern)
8. [Validation với FluentValidation](#8-validation-với-fluentvalidation)
9. [Xử Lý Exception](#9-xử-lý-exception)
10. [Authentication với JWT + Redis](#10-authentication-với-jwt--redis)

---

## 1. Tổng Quan Kiến Trúc

### Clean Architecture là gì?

Clean Architecture là kiến trúc phần mềm chia ứng dụng thành các **layers (tầng)** độc lập, với nguyên tắc:
- **Dependency Inversion**: Layer trong cùng KHÔNG phụ thuộc vào layer ngoài
- **Business Logic** nằm ở trung tâm, không bị ảnh hưởng bởi database hay UI

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│              (Controllers, Middlewares)                     │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                      │
│         (Database, Redis, External Services)                │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                        │
│           (Use Cases, DTOs, Interfaces)                     │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                           │
│         (Entities, Business Rules, Exceptions)              │
│                    ⭐ TRUNG TÂM ⭐                           │
└─────────────────────────────────────────────────────────────┘
```

### Tại sao dùng Clean Architecture?

| Lợi ích | Giải thích |
|---------|------------|
| **Testable** | Dễ viết unit test vì các layer độc lập |
| **Maintainable** | Thay đổi database không ảnh hưởng business logic |
| **Scalable** | Dễ mở rộng tính năng mới |
| **Framework Independent** | Domain không phụ thuộc vào framework |

---

## 2. Các Thư Viện Chính

| Thư viện | Mục đích | Ở đâu |
|----------|----------|-------|
| **MediatR** | Triển khai CQRS pattern | Application |
| **FluentValidation** | Validate input data | Application |
| **Entity Framework Core** | ORM - làm việc với database | Infrastructure |
| **Npgsql** | PostgreSQL driver | Infrastructure |
| **StackExchange.Redis** | Làm việc với Redis | Infrastructure |
| **BCrypt.Net** | Hash password | Infrastructure |
| **JWT Bearer** | Authentication | Infrastructure + API |
| **Serilog** | Logging | API |

---

## 3. Cấu Trúc Thư Mục

```
GreenDragonTrading.dotnet/
│
├── GreenDragonTrading.Domain/          # 🎯 TRUNG TÂM - Business Logic
│   ├── Entities/                        # Các entity (bảng trong DB)
│   │   └── User.cs
│   ├── Enums/                           # Enums
│   │   └── UserRole.cs
│   ├── Exceptions/                      # Custom exceptions
│   │   ├── DomainException.cs
│   │   ├── BusinessRuleException.cs
│   │   ├── NotFoundException.cs
│   │   ├── UnauthorizedException.cs
│   │   └── ConflictException.cs
│   └── Interfaces/                      # Định nghĩa contracts (interface)
│       ├── IGenericRepository.cs
│       ├── IUserRepository.cs
│       └── IUnitOfWork.cs
│
├── GreenDragonTrading.Application/      # 📝 USE CASES - Xử lý nghiệp vụ
│   ├── Common/
│   │   ├── Behaviors/                   # Pipeline behaviors
│   │   │   └── ValidationBehavior.cs
│   │   └── Models/
│   │       └── ApiResponse.cs           # Response wrapper
│   ├── DTOs/                            # Data Transfer Objects
│   │   └── Auth/
│   │       ├── AuthResponse.cs
│   │       ├── LoginRequest.cs
│   │       └── RegisterRequest.cs
│   ├── Interfaces/                      # Interface cho external services
│   │   ├── IJwtService.cs
│   │   ├── IPasswordHasher.cs
│   │   └── IRedisService.cs
│   ├── UseCases/                        # 🔥 CQRS Commands & Queries
│   │   └── Auth/
│   │       └── Commands/
│   │           ├── Register/
│   │           │   ├── RegisterCommand.cs
│   │           │   ├── RegisterCommandHandler.cs
│   │           │   └── RegisterCommandValidator.cs
│   │           ├── Login/
│   │           ├── Logout/
│   │           └── RefreshToken/
│   └── DependencyInjection.cs           # Đăng ký DI
│
├── GreenDragonTrading.Infrastructure/   # 🔧 TRIỂN KHAI - Database, Services
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs      # EF Core DbContext
│   │   ├── UnitOfWork.cs
│   │   ├── Configurations/              # Fluent API config
│   │   └── Repositories/
│   │       ├── GenericRepository.cs
│   │       └── UserRepository.cs
│   ├── Services/
│   │   ├── JwtService.cs
│   │   ├── PasswordHasher.cs
│   │   └── RedisService.cs
│   └── DependencyInjection.cs
│
└── GreenDragonTrading.Api/              # 🌐 API - Entry Point
    ├── Controllers/
    │   └── AuthController.cs
    ├── Middlewares/
    │   └── GlobalExceptionMiddleware.cs
    ├── Program.cs                        # App configuration
    └── appsettings.json
```

---

## 4. Flow Dữ Liệu - Ví Dụ Đăng Ký Tài Khoản

Hãy theo dõi luồng dữ liệu khi user gửi request đăng ký tài khoản:

```
┌──────────────────────────────────────────────────────────────────────────┐
│                           CLIENT (Postman, Frontend)                      │
│                                                                          │
│  POST /api/auth/register                                                 │
│  Body: { "username": "john", "email": "john@example.com", "password": "123" }
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  STEP 1: API Layer - AuthController.cs                                   │
│  ─────────────────────────────────────                                   │
│  [HttpPost("register")]                                                  │
│  public async Task<IActionResult> Register(RegisterRequest request)     │
│  {                                                                       │
│      var command = new RegisterCommand(                                  │
│          request.Username,                                               │
│          request.Email,                                                  │
│          request.Password                                                │
│      );                                                                  │
│      var result = await _mediator.Send(command);  // ◄── Gửi đến MediatR│
│      return Ok(ApiResponse.SuccessResponse(result));                     │
│  }                                                                       │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                                 │  _mediator.Send(command)
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  STEP 2: MediatR Pipeline - ValidationBehavior.cs                        │
│  ────────────────────────────────────────────────                        │
│  MediatR sẽ chạy qua các "Behaviors" (middleware) trước khi đến Handler  │
│                                                                          │
│  ValidationBehavior:                                                     │
│  - Tìm tất cả Validators cho RegisterCommand                             │
│  - Chạy RegisterCommandValidator                                         │
│  - Nếu có lỗi → throw ValidationException                                │
│  - Nếu pass → tiếp tục đến Handler                                       │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                                 │  Validation passed ✅
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  STEP 3: Application Layer - RegisterCommandHandler.cs                   │
│  ─────────────────────────────────────────────────────                   │
│  public async Task<AuthResponse> Handle(RegisterCommand request, ...)   │
│  {                                                                       │
│      // 1. Check username tồn tại                                        │
│      if (await _unitOfWork.Users.UsernameExistsAsync(request.Username))  │
│          throw new ConflictException("User", "Username", request.Username);│
│                                                                          │
│      // 2. Check email tồn tại                                           │
│      if (await _unitOfWork.Users.EmailExistsAsync(request.Email))        │
│          throw new ConflictException("User", "Email", request.Email);    │
│                                                                          │
│      // 3. Tạo user entity                                               │
│      var user = new User                                                 │
│      {                                                                   │
│          Username = request.Username,                                    │
│          Email = request.Email,                                          │
│          HashedPassword = _passwordHasher.Hash(request.Password),        │
│          Role = UserRole.User                                            │
│      };                                                                  │
│                                                                          │
│      // 4. Lưu vào database                                              │
│      await _unitOfWork.Users.AddAsync(user);                             │
│      await _unitOfWork.SaveChangesAsync();                               │
│                                                                          │
│      // 5. Generate JWT tokens                                           │
│      var accessToken = _jwtService.GenerateAccessToken(user);            │
│      var refreshToken = _jwtService.GenerateRefreshToken();              │
│                                                                          │
│      // 6. Lưu refresh token vào Redis                                   │
│      await _redisService.SetRefreshTokenAsync(user.Id, refreshToken);    │
│                                                                          │
│      // 7. Trả về response                                               │
│      return new AuthResponse(user.Id, user.Username, ...);               │
│  }                                                                       │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                                 │  _unitOfWork.Users.AddAsync(user)
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  STEP 4: Infrastructure Layer - UserRepository.cs                        │
│  ─────────────────────────────────────────────────                       │
│  public async Task AddAsync(User entity, CancellationToken ct)           │
│  {                                                                       │
│      await _context.Users.AddAsync(entity, ct);                          │
│  }                                                                       │
│                                                                          │
│  ─────────────────────────────────────────────────                       │
│  UnitOfWork.SaveChangesAsync() → _context.SaveChangesAsync()             │
│  Entity Framework sẽ generate SQL:                                       │
│                                                                          │
│  INSERT INTO users (id, username, email, hashed_password, role, ...)     │
│  VALUES (gen_random_uuid(), 'john', 'john@example.com', '$2a$...', 0, ...)│
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                                 │  SQL executed ✅
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  STEP 5: PostgreSQL Database                                             │
│  ───────────────────────────                                             │
│                                                                          │
│  Table: users                                                            │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │ id (UUID)  │ username │ email            │ hashed_password │ ... │   │
│  ├──────────────────────────────────────────────────────────────────┤   │
│  │ abc-123... │ john     │ john@example.com │ $2a$11$...      │ ... │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└────────────────────────────────┬─────────────────────────────────────────┘
                                 │
                                 │  Return AuthResponse
                                 ▼
┌──────────────────────────────────────────────────────────────────────────┐
│  STEP 6: Response được trả về Client                                     │
│  ────────────────────────────────────                                    │
│                                                                          │
│  HTTP 200 OK                                                             │
│  {                                                                       │
│      "success": true,                                                    │
│      "message": "Success",                                               │
│      "data": {                                                           │
│          "userId": "abc-123-...",                                        │
│          "username": "john",                                             │
│          "email": "john@example.com",                                    │
│          "accessToken": "eyJhbGciOiJIUzI1NiIs...",                       │
│          "refreshToken": "random-refresh-token-string",                  │
│          "expiresAt": "2025-11-29T12:30:00Z"                             │
│      }                                                                   │
│  }                                                                       │
└──────────────────────────────────────────────────────────────────────────┘
```

### Tóm tắt Flow:

```
Request → Controller → MediatR → ValidationBehavior → CommandHandler → Repository → Database
                                                                   ↑
                                                          Services (JWT, Redis, Hash)
```

---

## 5. Giải Thích Chi Tiết Từng Layer

### 5.1 Domain Layer (Tầng Trung Tâm)

**Mục đích**: Chứa business logic thuần túy, KHÔNG phụ thuộc vào bất kỳ framework nào.

```csharp
// Entities/User.cs - Đại diện cho bảng users trong database
public class User
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("username")]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    // ... các property khác
}
```

```csharp
// Interfaces/IUserRepository.cs - Contract cho repository
public interface IUserRepository : IGenericRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
}
```

**Đặc điểm**:
- Không có `using` đến EF Core hay bất kỳ framework nào
- Chỉ định nghĩa **interface**, không có implementation
- Exceptions riêng cho domain

---

### 5.2 Application Layer

**Mục đích**: Chứa use cases (CQRS), DTOs, interfaces cho external services.

```csharp
// UseCases/Auth/Commands/Register/RegisterCommand.cs
public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<AuthResponse>;  // IRequest từ MediatR
```

```csharp
// UseCases/Auth/Commands/Register/RegisterCommandValidator.cs
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}
```

**Đặc điểm**:
- Phụ thuộc vào Domain layer
- Định nghĩa interfaces cho services (IJwtService, IRedisService)
- Chứa Command, Query, Handler, Validator

---

### 5.3 Infrastructure Layer

**Mục đích**: Triển khai các interfaces, làm việc với database, external services.

```csharp
// Persistence/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Fluent API - cấu hình index
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}
```

```csharp
// Persistence/Repositories/UserRepository.cs
public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);
    }
}
```

**Đặc điểm**:
- Implement các interfaces từ Domain và Application
- Sử dụng EF Core, Redis, JWT libraries
- Chứa DbContext, Repositories, Services

---

### 5.4 API Layer

**Mục đích**: Entry point, xử lý HTTP request/response.

```csharp
// Controllers/AuthController.cs
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
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = new RegisterCommand(
            request.Username,
            request.Email,
            request.Password
        );
        
        var result = await _mediator.Send(command);
        return Ok(ApiResponse.SuccessResponse(result, "User registered successfully"));
    }
}
```

**Đặc điểm**:
- Controller rất mỏng (thin controller)
- Chỉ nhận request, gửi command đến MediatR
- Không chứa business logic

---

## 6. Entity Framework Code-First

### Code-First là gì?

Code-First là cách tiếp cận trong EF Core, bạn viết **C# classes trước**, sau đó EF Core tự động tạo database schema.

### Quy trình:

```
1. Viết Entity (C# class)     →    User.cs
                                      ↓
2. Tạo Migration              →    dotnet ef migrations add CreateUserTable
                                      ↓
3. Apply Migration            →    dotnet ef database update
                                      ↓
4. Database được tạo          →    PostgreSQL: Table "users" created
```

### Ví dụ Entity với Data Annotations:

```csharp
[Table("users")]  // Tên bảng trong database
public class User
{
    [Key]                                    // Primary Key
    [Column("id")]                           // Tên cột
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Auto-generate UUID
    public Guid Id { get; set; }

    [Required]                               // NOT NULL
    [MaxLength(50)]                          // VARCHAR(50)
    [Column("username")]
    public string Username { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; }
}
```

### Kết quả SQL được generate:

```sql
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    username VARCHAR(50) NOT NULL,
    created_at TIMESTAMP NOT NULL
);
```

### Commands thường dùng:

```bash
# Tạo migration mới
dotnet ef migrations add <TenMigration> -p GreenDragonTrading.Infrastructure -s GreenDragonTrading.Api

# Apply migration vào database
dotnet ef database update -p GreenDragonTrading.Infrastructure -s GreenDragonTrading.Api

# Xem SQL sẽ được generate (không apply)
dotnet ef migrations script -p GreenDragonTrading.Infrastructure -s GreenDragonTrading.Api
```

---

## 7. CQRS + MediatR Pattern

### CQRS là gì?

**CQRS** = Command Query Responsibility Segregation

- **Command**: Thay đổi dữ liệu (Create, Update, Delete)
- **Query**: Đọc dữ liệu (Read)

```
┌─────────────────────────────────────────────────────────────┐
│                      CQRS Pattern                           │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│   COMMAND (Write)              QUERY (Read)                 │
│   ┌─────────────┐              ┌─────────────┐              │
│   │ Register    │              │ GetUserById │              │
│   │ Login       │              │ GetAllUsers │              │
│   │ Logout      │              │ SearchUsers │              │
│   │ UpdateUser  │              └─────────────┘              │
│   │ DeleteUser  │                                           │
│   └─────────────┘                                           │
│         │                            │                      │
│         ▼                            ▼                      │
│   ┌─────────────┐              ┌─────────────┐              │
│   │  Database   │              │  Database   │              │
│   │   (Write)   │              │   (Read)    │              │
│   └─────────────┘              └─────────────┘              │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

### MediatR là gì?

MediatR là thư viện giúp implement **Mediator Pattern** - tách biệt sender và receiver.

```
Controller  ─────►  MediatR  ─────►  Handler
(Sender)           (Mediator)       (Receiver)
```

### Cấu trúc một UseCase:

```
UseCases/
└── Auth/
    └── Commands/
        └── Register/
            ├── RegisterCommand.cs           # Request object
            ├── RegisterCommandHandler.cs    # Xử lý logic
            └── RegisterCommandValidator.cs  # Validation rules
```

### Code mẫu:

```csharp
// 1. Command - Request object
public record RegisterCommand(
    string Username,
    string Email,
    string Password
) : IRequest<AuthResponse>;  // ◄── Kế thừa IRequest<TResponse>

// 2. Handler - Xử lý logic
public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    public async Task<AuthResponse> Handle(
        RegisterCommand request, 
        CancellationToken cancellationToken)
    {
        // Business logic here
        return new AuthResponse(...);
    }
}

// 3. Gọi từ Controller
var result = await _mediator.Send(new RegisterCommand(...));
```

### MediatR Pipeline:

```
Request → Behavior 1 → Behavior 2 → ... → Handler → Response
              ▲
              │
        ValidationBehavior
        (Chạy Validator trước khi đến Handler)
```

---

## 8. Validation với FluentValidation

### FluentValidation là gì?

Thư viện giúp định nghĩa validation rules một cách rõ ràng, dễ đọc.

### Ví dụ:

```csharp
public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters")
            .MaximumLength(50).WithMessage("Username must not exceed 50 characters")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters");
    }
}
```

### Kết hợp với MediatR Pipeline:

```csharp
// Common/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
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
        // 1. Chạy tất cả validators
        var context = new ValidationContext<TRequest>(request);
        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(result => result.Errors)
            .Where(f => f != null)
            .ToList();

        // 2. Nếu có lỗi → throw exception
        if (failures.Count != 0)
        {
            throw new ValidationException(failures);
        }

        // 3. Nếu pass → tiếp tục đến Handler
        return await next();
    }
}
```

---

## 9. Xử Lý Exception

### Custom Exceptions trong Domain:

```csharp
// Base exception
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

// Specific exceptions
public class ConflictException : DomainException { }      // 409 Conflict
public class NotFoundException : DomainException { }      // 404 Not Found
public class UnauthorizedException : DomainException { }  // 401 Unauthorized
public class BusinessRuleException : DomainException { }  // 400 Bad Request
```

### Global Exception Middleware:

```csharp
// Middlewares/GlobalExceptionMiddleware.cs
public class GlobalExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            ValidationException => HttpStatusCode.BadRequest,        // 400
            UnauthorizedException => HttpStatusCode.Unauthorized,    // 401
            NotFoundException => HttpStatusCode.NotFound,            // 404
            ConflictException => HttpStatusCode.Conflict,            // 409
            _ => HttpStatusCode.InternalServerError                  // 500
        };

        // Log và trả response
    }
}
```

### Flow xử lý lỗi:

```
Exception thrown  →  GlobalExceptionMiddleware  →  ApiResponse.FailResponse()
      ↓                        ↓                           ↓
ConflictException        Map to 409              { "success": false, "message": "..." }
```

---

## 10. Authentication với JWT + Redis

### Flow Authentication:

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        AUTHENTICATION FLOW                               │
└─────────────────────────────────────────────────────────────────────────┘

1. LOGIN
   ┌──────────┐     email + password     ┌─────────┐
   │  Client  │ ──────────────────────► │   API   │
   └──────────┘                          └────┬────┘
                                              │
                 ┌────────────────────────────┼────────────────────────────┐
                 │                            ▼                            │
                 │  Verify password → Generate tokens → Store in Redis     │
                 │                                                         │
                 │  Access Token (15 min)     Refresh Token (7 days)       │
                 │  ┌─────────────────┐       ┌─────────────────┐          │
                 │  │ JWT with userId │       │ Random string   │          │
                 │  │ + role + expiry │       │ stored in Redis │          │
                 │  └─────────────────┘       └─────────────────┘          │
                 └────────────────────────────┬────────────────────────────┘
                                              │
   ┌──────────┐   accessToken + refreshToken  │
   │  Client  │ ◄─────────────────────────────┘
   └──────────┘

2. ACCESS PROTECTED RESOURCE
   ┌──────────┐   Authorization: Bearer <accessToken>   ┌─────────┐
   │  Client  │ ──────────────────────────────────────► │   API   │
   └──────────┘                                          └────┬────┘
                                                              │
                 ┌────────────────────────────────────────────┤
                 │  JWT Middleware validates token             │
                 │  - Check signature                          │
                 │  - Check expiration                         │
                 │  - Extract claims (userId, role)            │
                 └────────────────────────────────────────────┤
                                                              │
   ┌──────────┐              Data                             │
   │  Client  │ ◄─────────────────────────────────────────────┘
   └──────────┘

3. REFRESH TOKEN
   ┌──────────┐   accessToken + refreshToken   ┌─────────┐
   │  Client  │ ──────────────────────────────►│   API   │
   └──────────┘                                 └────┬────┘
                                                     │
                 ┌───────────────────────────────────┤
                 │  1. Validate accessToken (expired OK)│
                 │  2. Extract userId                   │
                 │  3. Check refreshToken in Redis      │
                 │  4. Generate new tokens              │
                 │  5. Update Redis                     │
                 └───────────────────────────────────┤
                                                     │
   ┌──────────┐    New accessToken + refreshToken    │
   │  Client  │ ◄────────────────────────────────────┘
   └──────────┘

4. LOGOUT
   ┌──────────┐        userId        ┌─────────┐        ┌─────────┐
   │  Client  │ ───────────────────► │   API   │ ─────► │  Redis  │
   └──────────┘                      └─────────┘        └─────────┘
                                                    Delete refreshToken
```

### JWT Service:

```csharp
public class JwtService : IJwtService
{
    public string GenerateAccessToken(User user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),  // Short-lived
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

### Redis Service:

```csharp
public class RedisService : IRedisService
{
    public async Task SetRefreshTokenAsync(Guid userId, string refreshToken, TimeSpan expiry)
    {
        var key = $"refresh_token:{userId}";
        await _database.StringSetAsync(key, refreshToken, expiry);
    }

    public async Task<bool> ValidateRefreshTokenAsync(Guid userId, string refreshToken)
    {
        var key = $"refresh_token:{userId}";
        var storedToken = await _database.StringGetAsync(key);
        return storedToken == refreshToken;
    }

    public async Task RemoveRefreshTokenAsync(Guid userId)
    {
        var key = $"refresh_token:{userId}";
        await _database.KeyDeleteAsync(key);
    }
}
```

---

## 📝 Tổng Kết

### Điểm chính cần nhớ:

1. **Clean Architecture**: Domain ở trung tâm, không phụ thuộc framework
2. **CQRS**: Tách biệt Command (write) và Query (read)
3. **MediatR**: Mediator pattern, tách sender/receiver
4. **Code-First**: Viết C# entity → EF Core tạo database
5. **Repository Pattern**: Abstraction layer cho data access
6. **Unit of Work**: Quản lý transactions
7. **FluentValidation**: Validation rules rõ ràng
8. **Custom Exceptions**: Exception hierarchy cho domain
9. **JWT + Redis**: Stateless auth với refresh token

### Thứ tự đọc code đề xuất:

```
1. Domain/Entities/User.cs              → Hiểu cấu trúc data
2. Domain/Interfaces/IUserRepository.cs → Hiểu contract
3. Application/UseCases/Auth/Commands/  → Hiểu CQRS pattern
4. Infrastructure/Persistence/          → Hiểu implementation
5. Api/Controllers/AuthController.cs    → Hiểu entry point
6. Api/Program.cs                       → Hiểu DI configuration
```

---

## 🚀 Chạy Project

```bash
# 1. Đảm bảo PostgreSQL và Redis đang chạy

# 2. Update connection string trong appsettings.json

# 3. Tạo database
cd src/GreenDragonTrading.dotnet
dotnet ef database update -p GreenDragonTrading.Infrastructure -s GreenDragonTrading.Api

# 4. Chạy API
dotnet run --project GreenDragonTrading.Api

# 5. Test với Postman
POST http://localhost:5000/api/auth/register
{
    "username": "testuser",
    "email": "test@example.com",
    "password": "password123"
}
```

---

**Happy Coding! 🎉**
