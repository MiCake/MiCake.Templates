# AGENTS.md — MiCake RBAC Project Development Guide

> A quick-start guide for AI coding assistants.

## 1. Project Overview

ASP.NET Core Web API project built on the MiCake framework.
- Stack: PostgreSQL + EF Core, AutoMapper, FluentValidation, Serilog, JWT (Access + Refresh)

---

## 2. Architecture & Layering

Dependencies point **strictly inward** (outer layers may depend on inner layers; inner layers must never depend on outer ones). Put new code in the matching layer:

| Layer | Project | Responsibility |
|---|---|---|
| Web | `RBACWeb.Web` | Controllers, validators, `[RequirePermission]`, JWT/CORS host config |
| Application | `RBACWeb.Application` | Use cases: Services, Providers, Mapper, ErrorCodes, Audit |
| EFCore | `RBACWeb.EFCore` | `AppDbContext`, repository implementations |
| Domain | `RBACWeb.Domain` | Aggregate roots, entities, value objects, repository interfaces, domain services |
| Common | `RBACWeb.Common` | Cross-cutting utilities (encryption, time, cache, `OperationResult`) |
| Contracts | `RBACWeb.Contracts` | Pure DTOs and enums |

The module chain `WebModule → ApplicationModule → EFCoreModule → DomainModule → CommonModule` is already wired up; **only when adding a new assembly** do you need to create a module class and hook it up with `[RelyOn]`.

---

## 3. Core Conventions

### 3.1 Repositories & Unit of Work

- Repository interfaces live in the **Domain** layer (aggregate roots only); implementations live in **EFCore** and are auto-registered by `EFCoreModule` — no manual DI
- Extend `BaseRepository<T>` / `BasePagingRepository<T>` (EF Core repository + `long` key pre-wired); query via `GetDbSet(needTracking)` — pass `false` for read-only queries. **Add `bool needTracking = true` (before `CancellationToken`) only if the method serves both pure reads and mutation loads** (ref `PermissionRepo.GetByCodeAsync`); read-only methods hard-code `false` (ref `PermissionRepo.ExistsByCodeAsync`)
- **Never inject `AppDbContext` into a repository as a second entry point** — access data only through the repository's `DbSet`/`GetDbSet(...)`
- **Paging**: inherit `IRepositoryHasPagingQuery<T, long>` to get `PagingQueryAsync(PagingRequest, ...)` / `FilterPagingQueryAsync(...)`
- **Dynamic queries**: DTO implements `IDynamicQueryModel` with `[DynamicFilter]` attributes; call `query.GenerateFilterGroup()` to build the filter. **Prefer dynamic queries when there are many filter conditions (e.g., multi-criteria list filtering)**. See the official docs: [MiCake Dynamic Queries](https://micake.github.io/api/raw-markdown/en/utilities/query.txt)
- **Unit of Work** is automatic by default: commit on success, rollback on error. Declare explicitly with `[UnitOfWork(IsReadOnly = true)]` (read-only) or `[DisableUnitOfWork]`
- **On-demand includes**: use `FindAsync(id, includeFunc)` to include navigation properties on demand; avoid fixed full `Include(...)` chains
- Audit fields (`CreatedAt / CreatedBy / UpdatedAt / ModifiedBy`) are filled automatically by `AuditAggregateRoot` + JWT — no manual assignment

### 3.2 Services & DI

- Mark service classes with `[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]` for auto-registration — **no manual AddScoped**; use `[InjectService(typeof(IXxx), ...)]` when implementing an interface
- Never inject `IServiceProvider`; async methods always take a `CancellationToken` (controllers pass `HttpCancellationToken`)
- Use-case services → `Application/Services/<module>/`; single-purpose/external components (email, JWT issuance, etc.) → `Application/Providers/`

### 3.3 Results & Error Codes

- Service methods **always return** `OperationResult<T>` instead of throwing business exceptions: `OperationResult<T>.Success(data)` / `OperationResult<T>.Failure(msg, errorCode)`
- Error code = `{ModuleCode}.{ErrorCode}` (e.g., `06.9900`): module codes in `Web/Constants/ModuleCodes.cs`; error code classes in `Application/ErrorCodes/` extending `BaseErrorCodes`
- Controllers extend `BaseApiController` (inject `InfrastructureTools`), set `ModuleCode` in the constructor; return failures uniformly with `BadRequest(result.ErrorCode!, result.ErrorMessage)`
- **Success responses are wrapped automatically** by the framework (`Ok(data)` → `{Code, Message, Data}`). See the official docs: https://micake.github.io/api/raw-markdown/unified-response/overview.txt.
- **Simple reads may hit the repository directly from the Web layer**; business logic, transactions, and cross-aggregate operations go to an Application service

### 3.4 DTOs & Mapper

- Cross-layer shared DTOs → `Contracts/Dtos/<module>/`; host-only (validation/request views) → `Web/Dtos/<module>/`; enums → `Contracts/Enums/`
- Mapper Profiles: Domain ↔ DTO in `Application/Mapper/`, host-specific in `Web/Mapper/`. Both Web and Application assemblies are scanned — new Profiles apply automatically

### 3.5 Aggregate Root Encapsulation (DDD Rule)

- Keep property setters `private`; create via **static factories** (`Role.Create(...)`), mutate via **domain methods** (`role.Update(...)`, `role.Deactivate()`); collection navigation properties are read-only
- Value objects (`ContactInfo`, etc.) are configured with `OwnsOne` in `AppDbContext.OnModelCreating`
- Use structured logging with `ILogger<T>` (`_logger.LogInformation("msg {Id}", id)`); public APIs need XML `<summary>`; global usings live in each layer's `_usings.cs` — don't repeat them

---

## 4. Adding a Feature (Product module example)

Develop in order, using the code templates below:

**① Domain**: `Models/Product/Product.cs` (aggregate root) + `Repositories/IProductRepo.cs` (interface)

```csharp
public class Product : AuditAggregateRoot
{
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    protected Product() { }
    public static Product Create(string name, decimal price) => new() { Name = name, Price = price };
    public void UpdatePrice(decimal price) => Price = price;
}

public interface IProductRepo : IRepositoryHasPagingQuery<Product, long>
{
    Task<Product?> GetByNameAsync(string name, CancellationToken ct = default);
}
```

**② EFCore**: `Repositories/ProductRepo.cs` (implementation) + add `DbSet<Product>` to `AppDbContext` and configure indexes/constraints

```csharp
public class ProductRepo : BasePagingRepository<Product>, IProductRepo
{
    public ProductRepo(EFRepositoryDependencies<AppDbContext> dependencies) : base(dependencies) { }
    public async Task<Product?> GetByNameAsync(string name, CancellationToken ct = default)
        => await GetDbSet(false).FirstOrDefaultAsync(p => p.Name == name, ct);
}
```

**③ Application**: `Services/Product/ProductService.cs` (`[InjectService]`) + `Mapper/ProductMapper.cs` + error code class in `ErrorCodes/`

```csharp
[InjectService(Lifetime = MiCakeServiceLifetime.Scoped)]
public class ProductService
{
    private readonly IProductRepo _productRepo;
    private readonly IMapper _mapper;
    public ProductService(IProductRepo productRepo, IMapper mapper)
    {
        _productRepo = productRepo;
        _mapper = mapper;
    }

    public async Task<OperationResult<ProductDto?>> CreateAsync(CreateProductDto dto, CancellationToken ct = default)
    {
        var product = Product.Create(dto.Name, dto.Price);
        await _productRepo.AddAndGetIdAsync(product, ct);
        return OperationResult<ProductDto?>.Success(_mapper.Map<ProductDto>(product));
    }
}
```
If the service implements an interface, use `[InjectService(typeof(IXxx), ...)]` for DI registration.

**④ Contracts**: `Dtos/Product/ProductDto.cs` (if shared across layers)

**⑤ Web**: `Controllers/ProductController.cs` (extends `BaseApiController`) + FluentValidation validator + new module code in `ModuleCodes`

```csharp
using RBACWeb.Web.Authorization;   // namespace of the RequirePermission attribute

[Route("api/[controller]")]
[ApiController]
[Authorize]
[RequirePermission("product:view")]
public class ProductController : BaseApiController
{
    private readonly ProductService _productService;

    public ProductController(InfrastructureTools tools, ProductService productService) : base(tools)
    {
        _productService = productService;
        ModuleCode = ModuleCodes.ProductModule;
    }

    [HttpPost]
    [RequirePermission("product:create")]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateAsync(dto, HttpCancellationToken);
        return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorCode!, result.ErrorMessage);
    }
}
```

**⑥ Migration**: `dotnet ef migrations add AddProduct --project src/RBACWeb.EFCore --startup-project src/RBACWeb.Web`

**⑦ RBAC**: new permissions (`product:view`, etc.) must be inserted into the `Permission` table and assigned to roles before users can access

---

## 5. RBAC Essentials

- Authorization is based on `Permission.Code` (e.g., `order:create`); there is **no** `[RequireRole]` attribute
- Endpoint-level: `[RequirePermission("xxx:yyy")]` (controller/action level); in-code: inject `ICurrentUser` and call `HasPermissionAsync(code)`
- Data scope: `_currentUser.GetDataScopeAsync()` returns `All / Department / Self / Custom`; filter queries by type (Self → `CreatorId == current user`)
- `ICurrentUser` API: `GetCurrentUserId()`, `GetRoleIds()`, `GetPermissionsAsync()`, `HasPermissionAsync(code)`, `GetDataScopeAsync()`
- ⚠️ After changing role permissions/data scopes, you **must** call `InvalidateRoleCacheAsync(roleId)` or changes won't take effect (see `RoleService.UpdateAsync`)

---

## 6. Testing & Common Commands

- Test projects: `Domain.Tests` (aggregates/value objects), `Application.Tests` (services), `Web.Tests` (controllers/validators), `Web.IntegrationTests` (EF Core/UoW)
- Conventions: AAA pattern; naming `{Method}_{Scenario}_{ExpectedResult}`
- Commands: `dotnet build` / `dotnet test` / `dotnet run --project src/RBACWeb.Web` / `dotnet ef migrations add <Name> --project src/RBACWeb.EFCore --startup-project src/RBACWeb.Web`

## 7. AI Rules

- Never treat DTOs as entities — cross-layer values must go through DTOs

---

## 8. MiCake Official Docs

This project is built on MiCake. For unfamiliar MiCake APIs or concepts, consult the official knowledge base:

- **Official knowledge base**: [micake.github.io/llms.txt](https://micake.github.io/llms.txt)

> ⚠️ **When you don't understand a MiCake framework topic, get the answer from the official knowledge base ([micake.github.io/llms.txt](https://micake.github.io/llms.txt)) first — never guess or invent API usage.**
