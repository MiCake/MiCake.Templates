# Copilot Instructions for MiCake.Templates

## Project Overview

This is a **MiCake-based ASP.NET Core 10.0 DDD template** (StandardWeb) providing production-ready scaffolding for web APIs. The template uses the [MiCake framework](https://github.com/MiCake/MiCake) for modular architecture, dependency injection, and Entity Framework Core integration.

## Architecture (Dependency Flow: Web → Application → Domain → Common/Contracts)

| Layer | Project | Responsibility |
|-------|---------|----------------|
| **Web** | `StandardWeb.Web` | Controllers, HTTP concerns, startup, validators |
| **Application** | `StandardWeb.Application` | Services, providers, use-case orchestration |
| **Domain** | `StandardWeb.Domain` | Aggregates, repositories, EF Core DbContext |
| **Common** | `StandardWeb.Common` | Cross-cutting helpers (encryption, time, cache) |
| **Contracts** | `StandardWeb.Contracts` | Shared DTOs for cross-service contracts |

## Key Patterns

### Service Registration with `[InjectService]`
MiCake auto-registers services decorated with `[InjectService]`. No manual DI registration needed:
```csharp
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class AuthService : BaseLoginService { }
```

### Module System
Modules define dependencies using `[RelyOn]`. The chain is `WebModule → ApplicationModule → DomainModule`:
```csharp
[RelyOn(typeof(ApplicationModule), typeof(MiCakeAspNetCoreModule))]
public class WebModule : MiCakeModule { }
```

### Controller Pattern
Controllers inherit `BaseApiController` for standardized responses with module-prefixed error codes:
```csharp
public class AuthController : BaseApiController
{
    public AuthController(InfrastructureTools tools, AuthService authService) : base(tools)
    {
        ModuleCode = ModuleCodes.AuthModule; // "01"
    }
    
    // Use: return BadRequest(AuthErrorCodes.InvalidInput, "message");
    // Returns: { "code": "01.0001", "message": "..." }
}
```

### Error Codes
- Define module codes in `StandardWeb.Web/Constants/ModuleCodes.cs`
- Define error codes in `StandardWeb.Application/ErrorCodes/` inheriting `BaseErrorCodes`
- Format: `{ModuleCode}.{ErrorCode}` (e.g., `01.9900`)

### OperationResult Pattern
Services return `OperationResult<T>` for explicit success/failure handling:
```csharp
public async Task<OperationResult<UserDto?>> RegisterAsync(UserRegistrationDto data)
{
    if (invalid) return OperationResult<UserDto?>.Failure("message", ErrorCode);
    return OperationResult<UserDto?>.Success(dto);
}
```

### Repository Pattern
Repositories extend MiCake's `EFRepository` with the domain's `AppDbContext`:
```csharp
public abstract class BaseRepository<TAggregateRoot> 
    : EFRepository<AppDbContext, TAggregateRoot, long> { }
```

### DTO Placement Guidelines
The Contracts layer (`StandardWeb.Contracts`) is primarily for placing shared data contracts (DTOs) and public interfaces across processes/projects:

- **Place DTO in Contracts when:**
  - DTO serves as a public contract for APIs, shared by multiple hosts or consumers (e.g., client SDKs, external services).
  - DTO is shared across multiple projects (Web with other microservices/clients), requiring stability and backward compatibility.
  - DTO represents a "contract" — it contains no host or implementation details, only data for exchange.

- **Place DTO in Web layer when:**
  - DTO belongs only to the current API host (host view models/request validation models), involving input validation, display formats, or API-specific fields (e.g., containing hypermedia, pagination views, temporary tokens).
  - For scenarios where DTO is tightly coupled with business logic, define in Contracts layer (see below); if only for API layer presentation and binding, place in `StandardWeb.Web/Dtos`.

Examples:
- `PagingQueryUserDto` (used only in Web layer) → Place in `StandardWeb.Web/Dtos`.
- `UserDto` (as public user info display, possibly reused by other services or clients) → Place in `StandardWeb.Contracts/Dtos`.

General principle: If DTO belongs to "external contracts/cross-service sharing", place in Contracts; if needed only in Web layer (e.g., input validation, UI-specific fields), place in Web.

### AutoMapper Profile Placement Recommendations
Mapping configurations should be placed close to the types and boundaries they serve:

- **Place in Application layer (recommended for Domain ↔ Business DTO):**
  - Application layer handles use-cases and service orchestration, often needing to map domain models to business DTOs or internal transfer objects (e.g., Domain.User → Application.UserDto). Writing such Profiles in `StandardWeb.Application/Mapper` improves reusability and testability.

- **Place in Web layer (recommended for Web-layer-specific request/response DTOs):**
  - Web layer may need specific mappings for HTTP requests/responses (view models, additional fields, API versioning changes). Placing these Profiles in `StandardWeb.Web/Mapper` decouples mapping rules from the API layer.

## Coding Standards

### Web Layer / Application Layer Coding Standards
- Methods with business logic should be placed in Application layer services; Web layer only handles request processing and response return.
- Simple queries can directly call repositories in Web layer for processing and return, but complex business logic should be encapsulated in Application layer.

#### Examples
- Simple query by Id for User can directly call repository in Web layer:
```csharp
[HttpGet("{id}")]
public async Task<IActionResult> GetUserByIdAsync(long id)
{
    var user = await _userRepository.GetByIdAsync(id);
    if (user == null)
        return NotFound();
    var dto = _mapper.Map<UserDto>(user);
    return Ok(dto);
}
```

- Complex user registration logic should be in Application layer service:
```csharp
public async Task<OperationResult<UserDto?>> RegisterAsync(UserRegistrationDto data)
{
    if (invalid) return OperationResult<UserDto?>.Failure("message", ErrorCode);
    // Complex registration logic...
    return OperationResult<UserDto?>.Success(dto);
}
```

### Providers in Application Layer
The Providers path is used for placing services with single responsibilities or interacting with external systems, such as sending emails, generating JWT tokens, Azure clients, etc. Services in the Application layer coordinate these Providers to complete a complete business logic.

## Adding a New Feature (Recommended Process)
1. Define entities, aggregates, and repository interfaces in `StandardWeb.Domain`;
2. Add repository implementation in `StandardWeb.Domain`, and register/configure EF Core entities in `AppDbContext`;
3. Implement business logic (use-cases) in `StandardWeb.Application/Services/<Feature>/`, and write AutoMapper Profiles in Application layer;
4. Define error codes in `StandardWeb.Application\ErrorCodes` (if needed);
5. Define cross-service shared DTOs and interfaces in `StandardWeb.Contracts` (if needed);
6. Define module codes in `StandardWeb.Web\Constants\ModuleCodes.cs`;
7. Write API controllers and corresponding DTOs/Validators in `StandardWeb.Web/Controllers/<Feature>`;

**Example Controller**
```csharp
[Route("api/inventory")]
public class InventoryController : BaseApiController
{
    private readonly IInventoryService _service;

    public InventoryController(InfrastructureTools tools, IInventoryService service) : base(tools)
    {
        ModuleCode = ModuleCodes.InventoryModule;
        _service = service;
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] CreateInventoryDto dto)
    {
        var result = await _service.CreateAsync(dto, HttpCancellationToken);
        return result.IsSuccess
            ? Ok(result.Data, ErrorCodeDefinition.CreateInfoFailed)
            : BadRequest(ErrorCodeDefinition.OperationFailed, result.ErrorMessage);
    }
}
```

## Best Practices
- Design a unified error handling mechanism based on error codes, ensuring all errors have clear error codes and descriptions for easy handling by frontends and callers.
- Use Contracts layer and DTO layering reasonably to ensure clarity and stability of data contracts.
- When business logic is complex, prioritize encapsulating logic in Application layer services to keep Web layer simple and single-responsibility.
- Maintain clear module boundaries, avoid chaotic cross-layer dependencies, improve system maintainability and scalability, facilitating future evolution into a complete microservice architecture.
- Utilize unit tests and integration tests reasonably to ensure correctness and stability of each layer.

## Test Structure
- Unit tests: `tests/StandardWeb.Domain.Tests/`, `tests/StandardWeb.Application.Tests/`
- Integration tests: `tests/StandardWeb.Web.IntegrationTests/`
- Uses xUnit + Moq
