# MiCake.Templates

基于 [MiCake 框架](https://github.com/MiCake/MiCake/tree/refactor)的生产就绪项目模板集合，旨在帮助开发者快速搭建遵循领域驱动设计（DDD）原则的高质量 ASP.NET Core 应用程序。

## 🌟 特性

- **生产就绪**：基于最佳实践和真实场景构建
- **DDD 架构**：清晰的领域层、应用层和基础设施层分离
- **现代技术栈**：ASP.NET Core 9.0、EF Core、MySQL、JWT 认证
- **开发体验**：集成日志（Serilog + Seq）、API 文档（Scalar）、热重载支持
- **可扩展性**：模块化设计使得添加新功能和自定义变得简单

## 📦 可用模板

### StandardWeb
一个经过精心设计的 ASP.NET Core 启动模板，具有以下特性：
- **分层架构**：Domain、Application、Common、Web 层，依赖关系清晰
- **身份认证**：基于 JWT 的认证，支持刷新令牌
- **数据库**：MySQL + EF Core 迁移
- **API 文档**：OpenAPI（Swagger）+ Scalar UI
- **日志记录**：Serilog 与 Seq 集成
- **验证**：FluentValidation 请求验证
- **对象映射**：AutoMapper 用于 DTO 转换
- **错误处理**：标准化的错误码和响应

📖 **详细文档**：查看 [StandardWeb README（英文）](src/StandardWeb/README.en.md) 或 [StandardWeb README（中文）](src/StandardWeb/README.zh-CN.md)

## 🚀 快速开始

### 安装

1. **安装模板包**：
   ```bash
   dotnet new install .
   ```

2. **创建新项目**：
   ```bash
   dotnet new micake-standardweb -n YourProject.Name
   ```

3. **进入项目目录**：
   ```bash
   cd YourProject.Name/src/StandardWeb
   ```

4. **还原依赖项**：
   ```bash
   dotnet restore StandardWeb.sln
   ```

5. **配置数据库连接**：
   - 在 `StandardWeb.Web/appsettings.json` 中更新 MySQL 连接字符串
   - 设置 `AESEncryption:Key`（最少 16 个字符）

6. **应用数据库迁移**：
   ```bash
   dotnet ef database update --project StandardWeb.Web
   ```

7. **运行应用程序**：
   ```bash
   dotnet watch --project StandardWeb.Web
   ```

8. **访问 API 文档**：
   - 在浏览器中打开 `https://localhost:5001/scalar/v1`（或配置的端口）

## 🏗️ 架构概览

StandardWeb 模板遵循清晰的分层架构：

```
┌─────────────────────────────────────────────────┐
│  Web 层 (StandardWeb.Web)                       │
│  - 控制器、DTOs、启动配置                       │
└─────────────────┬───────────────────────────────┘
                  │ 依赖于
┌─────────────────▼───────────────────────────────┐
│  Application 层 (StandardWeb.Application)       │
│  - 服务、Providers、缓存、用例                  │
└─────────────────┬───────────────────────────────┘
                  │ 依赖于
┌─────────────────▼───────────────────────────────┐
│  Domain 层 (StandardWeb.Domain)                 │
│  - 实体、聚合、仓储、DbContext                  │
└─────────────────────────────────────────────────┘
         ┌────────┴────────┐
         │                 │
┌────────▼──────┐  ┌──────▼────────────┐
│ Common        │  │ CommonWebLib      │
│ (辅助类、     │  │ (基础控制器、     │
│  认证配置)    │  │  HTTP 客户端)     │
└───────────────┘  └───────────────────┘
```

### 各层职责

- **Web**：HTTP 请求处理、API 端点、请求/响应 DTOs
- **Application**：业务逻辑、编排、用例、缓存
- **Domain**：核心业务实体、领域逻辑、仓储接口
- **Common**：共享工具、辅助类和横切关注点
- **CommonWebLib**：可复用的 Web 基础设施组件

## 📝 配置

`appsettings.json` 中的关键配置节：

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=standardweb;User=root;Password=yourpassword;"
  },
  "Jwt": {
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "SecretKey": "your-secret-key-min-32-chars",
    "AccessTokenExpirationMinutes": 30,
    "RefreshTokenExpirationDays": 7
  },
  "AESEncryption": {
    "Key": "your-16-char-key!"
  },
  "AllowedOrigins": "https://yourdomain.com,https://*.yourdomain.com",
  "Serilog": {
    "Using": ["Serilog.Sinks.Seq"],
    "WriteTo": [
      { "Name": "Seq", "Args": { "serverUrl": "http://localhost:5341" } }
    ]
  }
}
```

## 🔧 添加新功能模块

按照以下步骤添加新的业务模块：

1. **定义领域模型**：在 `StandardWeb.Domain/Models/[YourModule]` 中
2. **创建仓储接口**：在 `StandardWeb.Domain/Repositories/Interfaces` 中
3. **实现仓储**：在 `StandardWeb.Domain/Repositories` 中
4. **创建应用服务**：在 `StandardWeb.Application/Services/[YourModule]` 中
5. **定义 DTOs**：在 `StandardWeb.Web/Dtos/[YourModule]` 中
6. **创建控制器**：在 `StandardWeb.Web/Controllers/[YourModule]Controller.cs` 中
7. **添加 AutoMapper Profile**：在 `StandardWeb.Web/Mapper/[YourModule]Profile.cs` 中
8. **添加验证器**：在 `StandardWeb.Web/Validators/[YourModule]` 中（如需要）

控制器示例结构：
```csharp
[Route("api/[controller]")]
public class ProductController : BaseApiController
{
    private readonly IProductService _service;

    public ProductController(InfrastructureTools tools, IProductService service) 
        : base(tools)
    {
        ModuleCode = "03"; // 唯一的模块代码
        _service = service;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _service.GetByIdAsync(id, HttpCancellationToken);
        return result.IsSuccess 
            ? Ok(result.Data) 
            : BadRequest(ErrorCodeDefinition.NotFound, result.ErrorMessage);
    }
}
```

## 🧪 测试

该模板设计为易于测试：

1. **单元测试**：隔离测试领域逻辑和服务
2. **集成测试**：使用内存数据库测试 API 端点
3. **端到端测试**：通过 HTTP 客户端测试完整工作流

测试结构示例：
```
YourProject.Tests/
├── Unit/
│   ├── Domain/
│   ├── Application/
│   └── Helpers/
├── Integration/
│   └── Controllers/
└── TestUtilities/
```

## 📚 其他资源

- **MiCake 框架**：[GitHub 仓库](https://github.com/MiCake/MiCake)
- **ASP.NET Core**：[官方文档](https://docs.microsoft.com/zh-cn/aspnet/core)
- **领域驱动设计**：[DDD 参考](https://www.domainlanguage.com/ddd/)
- **整洁架构**：[整洁架构指南](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 贡献

欢迎贡献！请随时提交问题或拉取请求以改进这些模板。

1. Fork 本仓库
2. 创建特性分支（`git checkout -b feature/AmazingFeature`）
3. 提交更改（`git commit -m 'Add some AmazingFeature'`）
4. 推送到分支（`git push origin feature/AmazingFeature`）
5. 打开 Pull Request

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

## 💬 支持

- **问题**：[GitHub Issues](https://github.com/MiCake/MiCake.Templates/issues)
- **讨论**：[GitHub Discussions](https://github.com/MiCake/MiCake.Templates/discussions)
- **MiCake 框架**：[MiCake 仓库](https://github.com/MiCake/MiCake)

## 🎯 路线图

- [ ] 添加更多模板变体（微服务、Blazor 等）
- [ ] 包含 Docker 和 Docker Compose 配置
- [ ] 添加示例测试项目
- [ ] 创建从现有项目迁移的指南
- [ ] 添加 CI/CD 管道模板（GitHub Actions、Azure DevOps）

---

**编码愉快！🎉**
