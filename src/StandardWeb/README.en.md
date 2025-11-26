# StandardWeb Template Description

StandardWeb is a ready-to-use ASP.NET Core 10.0 + DDD template built on the MiCake framework. It encapsulates common web service components (authentication, logging, EF Core, mapping, etc.) in layers for easy startup and enterprise expansion.

## Architecture Overview
The template adopts a clear layered design (using MiCake modular support, with layer dependency direction: Web → Application → Domain → Common/Contracts):

| Layer | Project | Main Responsibilities |
| --- | --- | --- |
| Web Host | `StandardWeb.Web` | Application startup and hosting: Program.cs, Controller, API Host configuration, OpenAPI/Swagger, authentication and middleware registration |
| Application | `StandardWeb.Application` | Application use-cases/business orchestration: Services (Use-cases), Providers (e.g., JwtProvider), DTO mapping (AutoMapper configuration) |
| Domain | `StandardWeb.Domain` | Domain models, aggregates, repository interfaces and implementations, EF Core DbContext and migrations |
| Common | `StandardWeb.Common` | Cross-layer infrastructure: Utility classes (encryption, time, etc.), shared contracts/types, authentication/Token helpers, cache/HttpClient encapsulation |
| Contracts | `StandardWeb.Contracts` | External DTOs (public data contracts), for sharing between layers (or other services) |

Dependencies flow from top to bottom (Web → Application → Domain), with common tools located in `StandardWeb.Common` for easy sharing across modules.

## Directory Structure (Simplified View)
```
src/StandardWeb
├── StandardWeb.Common/       # Cross-layer infrastructure: Encryption/decryption, time, Result, Token helpers, etc.
├── StandardWeb.Contracts/    # Shared DTOs/Contracts (connecting with Application, Web)
├── StandardWeb.Domain/       # Domain models, repository interfaces and implementations, AppDbContext
├── StandardWeb.Application/  # Business services, Providers, AutoMapper Profiles
├── StandardWeb.Web/          # Startup host: Program.cs, controllers, OpenAPI, Startup extensions
├── tests/                    # Test projects
│   ├── StandardWeb.Domain.Tests/
│   ├── StandardWeb.Application.Tests/
│   ├── StandardWeb.Web.Tests/
│   └── StandardWeb.Web.IntegrationTests/
└── tools/                    # Tool projects
    └── EfCoreMigrationApp/   # EF Core migration tool
```

## Quick Start (3 Minutes to Get Started)
Below are the minimal steps to experience the template. The template defaults to PostgreSQL (Npgsql), with DbContext registered via AddNpgsql in `Program.cs`.

1) Clone and switch to the template directory

```powershell
cd src/StandardWeb
dotnet restore
```

2) Build the solution

```powershell
dotnet build StandardWeb.sln
```

3) Configure (development environment)

- Set in `StandardWeb.Web/appsettings.Development.json` or environment variables:
    - `ConnectionStrings:DefaultConnection` → PostgreSQL connection string
    - `Jwt:Issuer`, `Jwt:Audience`, `Jwt:SecretKey` → For JWT verification
    - `AllowedOrigins` → For CORS, comma-separated, supports `https://*.example.com` wildcards
- Configure database connection string in `tools\EfCoreMigrationApp\appsettings.json` for database generation and migration

4) Apply EF Core migrations and initialize database

```powershell
cd tools\EfCoreMigrationApp
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5) Run the project

- Start the `StandardWeb.Web` project

6) View running results (during development)

In development mode, OpenAPI/Scalar documentation is enabled by default (controlled in Program.cs); after running `StandardWeb.Web`, it will default to `http://localhost:<port>/scalar/v1` to view interfaces and reference documentation.

## Contracts (Contract Layer) and DTO Placement Guidelines

The Contracts layer (`StandardWeb.Contracts`) is primarily for placing shared data contracts (DTOs) and public interfaces across processes/projects:

- Scenarios to place DTO in Contracts:
    - DTO serves as a public contract for APIs, shared by multiple hosts or consumers (e.g., client SDKs, external services).
    - DTO is shared across multiple projects (Web with other microservices/clients), requiring stability and backward compatibility.
    - DTO represents a "contract" — it contains no host or implementation details, only data for exchange.

- Scenarios to place DTO in Web layer:
    - DTO belongs only to the current API host (host view models/request validation models), involving input validation, display formats, or API-specific fields (e.g., containing hypermedia, pagination views, temporary tokens).
    - For scenarios where DTO is tightly coupled with business logic, define in Contracts layer (see below); if only for API layer presentation and binding, place in `StandardWeb.Web/Dtos`.

Examples:
- `PagingQueryUserDto` (used only in Web layer) → Can be placed in `StandardWeb.Web/Dtos`.
- `UserDto` (as public user info display, possibly reused by other services or clients) → Place in `StandardWeb.Contracts/Dtos`.

General principle: If DTO belongs to "external contracts/cross-service sharing", place in Contracts; if needed only in Web layer (e.g., input validation, UI-specific fields), place in Web.

## AutoMapper Profile Placement Recommendations

Mapping configurations should be placed close to the types and boundaries they serve:

- Scenarios to place in Application layer (recommended for Domain ↔ Business DTO):
    - Application layer handles use-cases and service orchestration, often needing to map domain models to business DTOs or internal transfer objects (e.g., Domain.User → Application.UserDto). Writing such Profiles in `StandardWeb.Application/Mapper` improves reusability and testability.

- Scenarios to place in Web layer (recommended for Web-layer-specific request/response DTOs):
    - Web layer may need specific mappings for HTTP requests/responses (view models, additional fields, API versioning changes). Placing these Profiles in `StandardWeb.Web/Mapper` decouples mapping rules from the API layer.

## Web Layer / Application Layer Coding Standards
- Methods with business logic should be placed in Application layer services; Web layer only handles request processing and response return.
- Simple queries can directly call repositories in Web layer for processing and return, but complex business logic should be encapsulated in Application layer.

### Examples
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

## Adding a New Feature Module (Recommended Process)
1. Define entities, aggregates, and repository interfaces in `StandardWeb.Domain`;
2. Add repository implementation in `StandardWeb.Domain`, and register/configure EF Core entities in `AppDbContext`;
3. Implement business logic (use-cases) in `StandardWeb.Application/Services/<Feature>/`, and write AutoMapper Profiles in Application layer;
4. Define error codes in `StandardWeb.Application\ErrorCodes` (if needed);
5. Define cross-service shared DTOs and interfaces in `StandardWeb.Contracts` (if needed);
6. Define module codes in `StandardWeb.Web\Constants\ModuleCodes.cs`;
7. Write API controllers and corresponding DTOs/Validators in `StandardWeb.Web/Controllers/<Feature>`;
8. Test and add migrations (if introducing new entities, run `dotnet ef migrations add` and update database in `tools\EfCoreMigrationApp`).

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
