# StandardWeb 模板说明

StandardWeb 是基于 MiCake 框架构建的一个开箱即用的 ASP.NET Core 9.0 + DDD 模板。它把常见的 Web 服务组成部分（认证、日志、EF Core、Docs、Validator、映射等）分层封装，便于快速上手并在企业场景中扩展。

## 架构概览
模板采用清晰的分层设计（并使用 MiCake 模块化支持，各层依赖方向为 Web → Application → Domain → Common/Contracts）：

| 层级 | 项目 | 主要职责 |
| --- | --- | --- |
| Web Host | `StandardWeb.Web` | 应用启动与宿主：Program.cs、Controller、API Host 配置、OpenAPI/Swagger、认证与中间件注册 |
| Application | `StandardWeb.Application` | 应用用例/业务编排：服务（Use-cases）、Providers（如 JwtProvider）、DTO 映射（AutoMapper 配置） |
| Domain | `StandardWeb.Domain` | 领域模型、聚合根、仓储接口与实现、EF Core DbContext 与迁移 |
| Common | `StandardWeb.Common` | 跨层基础设施：工具类（加密、时间等）、共享 contract/类型、认证/Token 辅助、缓存/HttpClient 封装 |
| Contracts | `StandardWeb.Contracts` | 对外 DTO（公共数据契约），供不同层之间（或其他服务）共享使用 |

依赖自上而下流动（Web → Application → Domain），公共工具位于 `StandardWeb.Common`，方便被多个模块共享。

## 快速开始（3 分钟上手）
下面是快速体验模板的最小步骤。模板默认使用 PostgreSQL（Npgsql），并通过 `Program.cs` 中的 AddNpgsql 注册 DbContext。

1) 克隆并切换到模板目录

```powershell
cd src/StandardWeb
dotnet restore
```

2) 构建解决方案

```powershell
dotnet build StandardWeb.sln
```

3) 配置（开发环境）

- 在 `StandardWeb.Web/appsettings.Development.json` 或环境变量中设置：
    - `ConnectionStrings:DefaultConnection` → PostgreSQL 连接字符串（示例：Host=localhost;Database=standardweb;Username=postgres;Password=secret）
    - `Jwt:Issuer`, `Jwt:Audience`, `Jwt:SecretKey` → 用于 JWT 验证
    - （注）模板不再强制依赖 `AESEncryption:Key`；如需对称加解密，可在 `StandardWeb.Common` 中实现并通过配置注入。
    - `AllowedOrigins` → 用于 CORS，逗号分隔，支持 `https://*.example.com` 通配

4) 应用 EF Core 迁移并初始化数据库

```powershell
dotnet ef database update --project .\StandardWeb.Web\StandardWeb.Web.csproj
```

5) 运行（热重载）

```powershell
dotnet watch --project .\StandardWeb.Web\StandardWeb.Web.csproj
```

6) 打开 API 文档（开发时）

在开发环境运行时，会启用 OpenAPI/Scalar 文档（Program.cs 中控制）；访问 `http://localhost:<port>/swagger` 或 `http://localhost:<port>/openapi` 来查看接口和参考文档。

## 目录结构（精简视图）
```
src/StandardWeb
├── StandardWeb.Common/       # 跨层基础设施：加解密、时间、Result、Token 辅助等
├── StandardWeb.Contracts/    # 共享 DTO/Contracts（与 Application、Web 对接）
├── StandardWeb.Domain/       # 领域模型、仓储接口与实现、AppDbContext
├── StandardWeb.Application/  # 业务服务、Provider、AutoMapper Profile
└── StandardWeb.Web/          # 启动宿主：Program.cs、控制器、OpenAPI、Startup 扩展
```

## Contracts（契约层）与 DTO 放置指南

Contracts 层 (`StandardWeb.Contracts`) 的主要用途是放置跨进程/跨项目的共享数据契约（DTOs）和公共接口：

- 把 DTO 放到 Contracts 的场景：
    - DTO 用作对外 API 的公共契约，需要被多个宿主或消费者共享（例如：客户端 SDK、外部服务调用）。
    - DTO 在多个项目之间共享（Web 与其他微服务/客户端），需要保持稳定性与向后兼容。
    - DTO 表示“契约” —— 它不包含主机或实现细节，仅为数据交换而设计。

- 把 DTO 放到 Web 层的场景：
    - DTO 只属于当前的 API 主机（主机视图模型 / 请求校验模型），牵涉到输入验证、展示格式或 API 特有字段（例如包含 hypermedia、分页视图、临时 token 字段等）。
    - DTO 与业务逻辑紧密耦合的场景请在 Application 层定义（见下文），若仅为 API 层展现和绑定，请放在 `StandardWeb.Web/Dtos`。

示例：
- `LoginRequestDto`（仅用于 API 请求验证）→ 可放在 `StandardWeb.Web/Dtos`。
- `UserDto`（作为公共用户信息展示且可能被其他服务或客户端复用）→ 放在 `StandardWeb.Contracts/Dtos`。

通常遵循原则：如果 DTO 属于“对外契约/跨服务共享”则放在 Contracts；如果只在宿主层需要且实现细节不同（例如输入校验、UI 专用字段），则放在 Web。

## AutoMapper Profile 放置建议

映射配置的位置应当靠近它所服务的类型与边界：

- 放在 Application 层的情景（推荐用于领域 <-> 业务 DTO）：
    - Application 层负责用例与服务编排，常需要将领域模型映射为业务 DTO 或内部传递对象（例如：Domain.User -> Application.UserDto）。将此类 Profile 写在 `StandardWeb.Application/Mapper` 中可提高重用性和测试性。

- 放在 Web 层的情景（推荐用于主机特有的请求/响应 DTO）：
    - Web 层可能需要针对 HTTP 请求/响应（视图模型、附带字段、API 版本化变化）做特定映射。把这些 Profile 放在 `StandardWeb.Web/Mapper` 能把映射规则与 API 层解耦。

- 其它提示：
    - 若映射涉及 Contracts 层 DTO（跨宿主契约），把映射放在 Application 层更合适（领域到契约的映射是业务层面的转换）；
    - 避免在 Domain 层包含对 AutoMapper 的依赖；Domain 应保持纯粹，只包含领域逻辑。
    - 保持映射规则尽量小而清晰——如果映射大量逻辑且涉及验证/业务规则，把这些逻辑放到 Application 服务而不是 AutoMapper Profile 中。

总结：Profile 与 DTO 的放置规则应与 DTO 的作用范围一致 —— Contracts DTO 放在 Contracts、跨层业务映射放在 Application、主机/展示专有映射放在 Web。

约定说明：
- `AppDbContext` 公开聚合根 DbSet，便于 EF Core 迁移与查询清晰；
- `WebServiceRegistration` 将常用服务（Controllers、AutoMapper、FluentValidation、Options）统一注册，Program.cs 更简洁；
- 启动时会校验关键配置（如 `Jwt`）以便尽早发现缺失配置；
- 模块化：使用 MiCake 模块（WebModule → ApplicationModule → DomainModule）实现模块化、自动注册仓储等功能；
- 建议：控制器尽量薄，业务逻辑放在 Application 层的服务里，Domain 层包含聚合与仓储实现。

## 扩展一个业务模块（建议流程）
1. 在 `StandardWeb.Domain` 中定义实体、聚合及仓储接口（Contracts / Repositories）；
2. 在 `StandardWeb.Domain` 添加仓储实现（如果需要 EF），DomainModule 会自动注册仓储；
3. 在 `StandardWeb.Application/Services/<模块名>` 实现业务逻辑（Use-cases），并在 Application 层编写 AutoMapper Profile；
4. 在 `StandardWeb.Web/Controllers/<模块名>` 编写 API 控制器和对应 DTO / Validator，模板会自动扫描并注入；
5. 测试并添加迁移（如引入新实体要运行 `dotnet ef migrations add` 并 update 数据库）。

**示例 Controller**
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

## 配置检查清单
- `ConnectionStrings:DefaultConnection`：EF Core 使用（模板示例使用 PostgreSQL）；
- `Jwt`：Issuer、Audience 与 SecretKey（用于 `AddJWTAuthentication`）
-- 注：模板不再依赖 `AESEncryption:Key`。如需对称加密，可在 `StandardWeb.Common` 中实现并添加配置说明。
- `AllowedOrigins`：CORS 白名单，逗号分隔；支持 `https://*.contoso.com` 的通配规则（注意不要与 AllowCredentials() 一起使用 `*`）

## 常见问题
- **JWT 或关键配置缺失**：应用在启动时会尽早校验关键配置（例如 `Jwt`），若启动时报错请检查 `appsettings` 与环境变量；
- **浏览器提示 CORS**：确认前端 origin 与 `AllowedOrigins` 完全匹配（含协议与端口），并谨慎使用通配符/AllowCredentials；
- **刷新 Token 失败**：检查 `UserTokens` 表是否存在有效记录，模板已提供刷新 token 的基本实现，但在生产中建议使用 refresh-token 轮换且对 refresh token 进行哈希存储；

## 后续建议
- 在模板中增加 CI（dotnet test + dotnet format）并加入 GitHub Actions 或其他 CI 平台；
- 可以将模板打包并发布到私有 NuGet（`dotnet pack` + `dotnet new -i`）以便团队统一安装；
- 为本地体验准备 docker-compose（PostgreSQL、Seq 等），简化 demo/演示环境搭建；
- 考虑：统一响应格式 / ProblemDetails、完善全局异常处理、加强 Token 安全（rotation、哈希）以及增加健康检查、监控埋点等。
