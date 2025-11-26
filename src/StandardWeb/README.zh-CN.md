# StandardWeb 模板说明

StandardWeb 是基于 MiCake 框架构建的一个开箱即用的 ASP.NET Core 10.0 + DDD 模板。它把常见的 Web 服务组成部分（认证、日志、EF Core、映射等）分层封装，便于快速上手并在企业场景中扩展。

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

## 目录结构（精简视图）
```
src/StandardWeb
├── StandardWeb.Common/       # 跨层基础设施：加解密、时间、Result、Token 辅助等
├── StandardWeb.Contracts/    # 共享 DTO/Contracts（与 Application、Web 对接）
├── StandardWeb.Domain/       # 领域模型、仓储接口与实现、AppDbContext
├── StandardWeb.Application/  # 业务服务、Provider、AutoMapper Profile
├── StandardWeb.Web/          # 启动宿主：Program.cs、控制器、OpenAPI、Startup 扩展
├── tests/                    # 测试项目
│   ├── StandardWeb.Domain.Tests/
│   ├── StandardWeb.Application.Tests/
│   ├── StandardWeb.Web.Tests/
│   └── StandardWeb.Web.IntegrationTests/
└── tools/                    # 工具项目
    └── EfCoreMigrationApp/   # EF Core 迁移工具
```

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
    - `ConnectionStrings:DefaultConnection` → PostgreSQL 连接字符串
    - `Jwt:Issuer`, `Jwt:Audience`, `Jwt:SecretKey` → 用于 JWT 验证
    - `AllowedOrigins` → 用于 CORS，逗号分隔，支持 `https://*.example.com` 通配
- 在 `tools\EfCoreMigrationApp\appsettings.json` 中配置数据库连接字符串用于数据库生成和迁移

4) 应用 EF Core 迁移并初始化数据库

```powershell
cd tools\EfCoreMigrationApp
dotnet ef migrations add InitialCreate
dotnet ef database update
```

5) 运行项目

- 启动 `StandardWeb.Web` 项目

6) 查看运行效果（开发时）

在开发环境运行时，会默认启用 OpenAPI/Scalar 文档（Program.cs 中控制）；当运行`StandardWeb.Web`后，默认会跳转到 `http://localhost:<port>/scalar/v1` 来查看接口和参考文档。

## Contracts（契约层）与 DTO 放置指南

Contracts 层 (`StandardWeb.Contracts`) 的主要用途是放置跨进程/跨项目的共享数据契约（DTOs）和公共接口：

- 把 DTO 放到 Contracts 的场景：
    - DTO 用作对外 API 的公共契约，需要被多个宿主或消费者共享（例如：客户端 SDK、外部服务调用）。
    - DTO 在多个项目之间共享（Web 与其他微服务/客户端），需要保持稳定性与向后兼容。
    - DTO 表示“契约” —— 它不包含主机或实现细节，仅为数据交换而设计。

- 把 DTO 放到 Web 层的场景：
    - DTO 只属于当前的 API 主机（主机视图模型 / 请求校验模型），牵涉到输入验证、展示格式或 API 特有字段（例如包含 hypermedia、分页视图、临时 token 字段等）。
    - DTO 与业务逻辑紧密耦合的场景请在 Contracts 层定义（见下文），若仅为 API 层展现和绑定，请放在 `StandardWeb.Web/Dtos`。

示例：
- `PagingQueryUserDto`（仅仅在Web层使用）→ 可放在 `StandardWeb.Web/Dtos`。
- `UserDto`（作为公共用户信息展示且可能被其他服务或客户端复用）→ 放在 `StandardWeb.Contracts/Dtos`。

通常遵循原则：如果 DTO 属于“对外契约/跨服务共享”则放在 Contracts；如果只在Web层需要（例如输入校验、UI 专用字段），则放在 Web。

## AutoMapper Profile 放置建议

映射配置的位置应当靠近它所服务的类型与边界：

- 放在 Application 层的情景（推荐用于领域 <-> 业务 DTO）：
    - Application 层负责用例与服务编排，常需要将领域模型映射为业务 DTO 或内部传递对象（例如：Domain.User -> Application.UserDto）。将此类 Profile 写在 `StandardWeb.Application/Mapper` 中可提高重用性和测试性。

- 放在 Web 层的情景（推荐用于Web层特有的请求/响应 DTO）：
    - Web 层可能需要针对 HTTP 请求/响应（视图模型、附带字段、API 版本化变化）做特定映射。把这些 Profile 放在 `StandardWeb.Web/Mapper` 能把映射规则与 API 层解耦。

## Web 层 / Application 层 编码规范
- 具有业务逻辑的方法应放在 Application 层的服务中，Web 层仅负责请求处理与响应返回。
- 简单的查询可以直接在Web层调用仓储进行处理和返回，但复杂业务逻辑应封装在 Application 层。

### 示例
+ 简单根据Id来查询User，可以直接在Web层调用仓储：
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

+ 复杂的注册用户逻辑应放在 Application 层的服务中：
```csharp
public async Task<OperationResult<UserDto?>> RegisterAsync(UserRegistrationDto data)
{
    if (invalid) return OperationResult<UserDto?>.Failure("message", ErrorCode);

    // 复杂注册逻辑...
    return OperationResult<UserDto?>.Success(dto);
}
```

### Application 层中的 Providers
Providers路径用于放置各种职责单一或者与外界交互的服务类，例如发送邮件、生成 JWT Token、Azure Client 等。Application 层下的Service通过协调这些Providers来完成一项完整的业务逻辑。

## 扩展一个业务模块（建议流程）
1. 在 `StandardWeb.Domain` 中定义实体、聚合及仓储接口等领域对象；
2. 在 `StandardWeb.Domain` 添加仓储实现，并且在`AppDbContext`中注册和配置EFCore实体；
3. 在 `StandardWeb.Application/Services/<模块名>` 实现业务逻辑（Use-cases），并在 Application 层编写 AutoMapper Profile；
4. 在`StandardWeb.Application\ErrorCodes`中定义错误码；（如果需要）
5. 在`StandardWeb.Contracts`中定义跨服务共享的 DTO 和接口；（如果需要）
6. 在 `StandardWeb.Web\Constants\ModuleCodes.cs` 中定义模块码；
7. 在 `StandardWeb.Web/Controllers/<模块名>` 编写 API 控制器和对应 DTO / Validator；
8. 测试并添加迁移（如引入新实体要在`tools\EfCoreMigrationApp`中运行 `dotnet ef migrations add` 并 update 数据库）。

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

## 实践建议
+ 基于异常错误码设计统一的错误处理机制，确保所有错误都有明确的错误码和描述，便于前端和调用方处理。
+ 合理使用Contracts层与DTO分层，确保数据契约的清晰与稳定。
+ 在业务逻辑复杂时，优先考虑将逻辑封装在 Application 层的服务中，保持 Web 层的简洁和职责单一。
+ 保持模块边界清晰，避免跨层依赖混乱，提升系统的可维护性和扩展性。便于未来演进为完善的微服务架构。
+ 合理利用单元测试和集成测试，确保各层功能的正确性和稳定性。