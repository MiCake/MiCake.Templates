# StandardWeb 模板说明

StandardWeb 基于 MiCake 框架，提供开箱即用的 ASP.NET Core 9.0 + DDD 架构脚手架。开发者可以通过 `dotnet new micake-standardweb` 快速创建具备认证、日志、Swagger/Scalar 文档的后台服务。

## 架构概览
| 层级 | 项目 | 主要职责 |
| --- | --- | --- |
| Domain | `StandardWeb.Domain` | 实体、聚合根、仓储接口、EF Core 配置 |
| Application | `StandardWeb.Application` | 业务用例、服务、Provider、缓存封装 |
| Common | `StandardWeb.Common` / `CommonWebLib` | JWT、加密、HTTP Client、防腐层、基础控制器 |
| Web Host | `StandardWeb.Web` | ASP.NET Core 承载、认证接入、API/DTO/启动代码 |

依赖自上而下流动（Web → Application → Domain），公共工具位于 `StandardWeb.Common`，方便被多个模块共享。

## 快速开始
```bash
# 1. 创建解决方案
cd src
 dotnet new micake-standardweb -n Contoso.StandardWeb

# 2. 还原并构建
cd Contoso.StandardWeb/src/StandardWeb
 dotnet build StandardWeb.sln

# 3. 初始化数据库（MySQL 示例）
dotnet ef database update --project StandardWeb.Web

# 4. 启动开发服务器
dotnet watch --project StandardWeb.Web
```

## 目录结构
```
src/StandardWeb
├── CommonWebLib/          # 控制器基类、认证、Polly/HttpClient
├── StandardWeb.Common/    # 加解密、时间、通用结果模型
├── StandardWeb.Domain/    # 领域模型、仓储、DbContext
├── StandardWeb.Application/ # 应用服务、Provider、缓存
└── StandardWeb.Web/       # Web Host、DTO、控制器、启动配置
```

约定说明：
- `AppDbContext` 公开所有聚合根 DbSet，避免 EF Shadow Set；
- `WebServiceRegistration` 统一封装控制器、AutoMapper、FluentValidation、选项配置；
- `AESEncryptionOptions`、`JwtConfigOptions` 会在启动阶段校验，缺失配置会立即抛错。

## 扩展一个业务模块
1. 在 `StandardWeb.Domain` 中定义实体和仓储接口；
2. 在 `StandardWeb.Application/Services/<模块名>` 实现应用服务；
3. 在 `StandardWeb.Web/Controllers/<模块名>` 暴露 API，并编写 DTO/Validator；
4. AutoMapper Profile、FluentValidation Validator 放在同一程序集，`AddWebApiDefaults` 已自动扫描。

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
- `ConnectionStrings:DefaultConnection`：EF Core 使用的 MySQL 连接；
- `Jwt`：Issuer/Audience/SecretKey，供 `AddJWTAuthentication` 使用；
- `AESEncryption:Key`：必须至少 16 位；
- `AllowedOrigins`：逗号分隔的前端地址，支持 `https://*.contoso.com` 形式的通配；

## 常见问题
- **启动时报 AESEncryption Key**：检查 `appsettings` 是否配置 16+ 字符的 Key；
- **浏览器提示 CORS**：确认真实域名与 `AllowedOrigins` 完全匹配（含协议、端口）；
- **刷新 Token 失败**：检查 `UserTokens` 表是否存在过期记录，必要时清理旧数据。

## 后续建议
- 使用 `dotnet format` 与 CI 任务统一编码规范；
- 将模板打包发布到私有 NuGet（`dotnet pack` + `dotnet new -i`），方便团队统一使用；
- 结合容器（如 docker compose）预置 MySQL、Seq 等依赖，提升试用体验。
