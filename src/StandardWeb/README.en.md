# StandardWeb Template

MiCake StandardWeb delivers a pragmatic DDD starter for ASP.NET Core 9.0. It bundles Domain, Application, Common infrastructure, and Web host layers so new services can be created with `dotnet new micake-standardweb` and immediately expose secured APIs.

## Architecture Snapshot
| Layer | Project | Responsibility |
| --- | --- | --- |
| Domain | `StandardWeb.Domain` | Aggregate roots, repositories, EF Core mappings |
| Application | `StandardWeb.Application` | Use-cases (services), providers, cache helpers |
| Common | `StandardWeb.Common` / `CommonWebLib` | Cross-cutting helpers (JWT, encryption, HTTP clients, controller base) |
| Web Host | `StandardWeb.Web` | ASP.NET Core entry point, authentication, Swagger/Scalar docs |

> Dependency always flows downward (Web → Application → Domain). Shared utilities live in `StandardWeb.Common` to avoid circular references.

## Quick Start
```bash
# 1. Scaffold a new solution
cd src
 dotnet new micake-standardweb -n Contoso.StandardWeb

# 2. Restore and build
cd Contoso.StandardWeb/src/StandardWeb
 dotnet build StandardWeb.sln

# 3. Apply database migrations (MySQL example)
dotnet ef database update --project StandardWeb.Web

# 4. Run with hot reload
dotnet watch --project StandardWeb.Web
```

## Project Layout
```
src/StandardWeb
├── CommonWebLib/          # Controller base, shared HTTP policies, authentication
├── StandardWeb.Common/    # Primitive helpers (encryption, time, results)
├── StandardWeb.Domain/    # Entities, aggregates, repositories, DbContext
├── StandardWeb.Application/# Services, providers, cache abstractions
└── StandardWeb.Web/       # Host (Program.cs), API modules, DTOs
```

Key conventions:
- DbContext exposes every aggregate set (`User`, `ExternalLoginProvider`, `UserToken`, ...).
- Service registrations are grouped in `WebServiceRegistration` to keep `Program.cs` declarative.
- Options are validated at startup (`AESEncryptionOptions`, `JwtConfigOptions`) to fail fast.

## Adding a Feature Module
1. Create a folder under `StandardWeb.Application/Services/<Feature>` and implement the use case.
2. Define aggregates/repositories in `StandardWeb.Domain`.
3. Expose DTOs + controllers inside `StandardWeb.Web/Controllers/<Feature>`.
4. Hook AutoMapper profiles/validators by placing them in the same assembly—`AddWebApiDefaults` already scans it.

**Controller example**
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

## Configuration Checklist
- `ConnectionStrings:DefaultConnection` → MySQL server for EF Core.
- `Jwt` → Issuer, audience, symmetric key for `AddJWTAuthentication`.
- `AESEncryption:Key` → validated at startup; required for `EncryptionHelper`.
- `AllowedOrigins` → comma-separated list or `*` for development. Wildcards such as `https://*.contoso.com` are supported.

## Troubleshooting
- **Startup failure: "AESEncryption:Key must be configured"** → ensure appsettings contains a 16+ character key.
- **CORS blocked** → verify origin list (wildcards allowed) and that frontend origin matches protocol + port.
- **Token refresh errors** → check `UserTokens` table; stale entries mean cache/database mismatch.

## Next Steps
- Add CI (`dotnet test` + `dotnet format`) to enforce style.
- Package this template with `dotnet pack` so it can be installed globally (`dotnet new -i`).
