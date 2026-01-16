# MiCake.Templates

A collection of production-ready project templates based on the [MiCake framework](https://github.com/MiCake/MiCake/tree/refactor), designed to help developers quickly scaffold high-quality ASP.NET Core applications following Domain-Driven Design (DDD) principles.

## 🌟 Features

- **Production-Ready**: Built with best practices and real-world scenarios in mind
- **DDD Architecture**: Clear separation of Domain, Application, and Infrastructure layers
- **Modern Stack**: ASP.NET Core 9.0, EF Core, MySQL, JWT Authentication
- **Developer Experience**: Integrated logging (Serilog + Seq), API documentation (Scalar), hot reload support
- **Extensible**: Modular design makes it easy to add new features and customize for your needs

## 📦 Available Templates

### StandardWeb
An opinionated ASP.NET Core starter template featuring:
- **Layered Architecture**: Domain, Application, Common, Web layers with clear dependencies
- **Authentication**: JWT-based auth with refresh token support
- **Database**: MySQL with EF Core migrations
- **API Documentation**: OpenAPI (Swagger) + Scalar UI
- **Logging**: Serilog with Seq integration
- **Validation**: FluentValidation for request validation
- **Mapping**: AutoMapper for DTO conversions
- **Error Handling**: Standardized error codes and responses

📖 **Detailed Documentation**: See [StandardWeb README (English)](src/StandardWeb/README.en.md) or [StandardWeb README (Chinese)](src/StandardWeb/README.zh-CN.md)

## 🚀 Quick Start

### Installation

1. **Install the template package**:
   ```bash
   dotnet new install .
   ```

2. **Create a new project**:
   ```bash
   dotnet new micake-standardweb -n YourProject.Name
   ```

3. **Navigate to the project**:
   ```bash
   cd YourProject.Name/src/StandardWeb
   ```

4. **Restore dependencies**:
   ```bash
   dotnet restore StandardWeb.sln
   ```

5. **Configure database connection**:
   - Update `StandardWeb.Web/appsettings.json` with your MySQL connection string
   - Set the `AESEncryption:Key` (minimum 16 characters)

6. **Apply database migrations**:
   ```bash
   dotnet ef database update --project StandardWeb.Web
   ```

7. **Run the application**:
   ```bash
   dotnet watch --project StandardWeb.Web
   ```

8. **Access API documentation**:
   - Open your browser to `https://localhost:5001/scalar/v1` (or the configured port)

## 🏗️ Architecture Overview

The StandardWeb template follows a clean layered architecture:

```
┌─────────────────────────────────────────────────┐
│  Web Layer (StandardWeb.Web)                    │
│  - Controllers, DTOs, Startup configuration     │
└─────────────────┬───────────────────────────────┘
                  │ depends on
┌─────────────────▼───────────────────────────────┐
│  Application Layer (StandardWeb.Application)    │
│  - Services, Providers, Cache, Use Cases        │
└─────────────────┬───────────────────────────────┘
                  │ depends on
┌─────────────────▼───────────────────────────────┐
│  Domain Layer (StandardWeb.Domain)              │
│  - Entities, Aggregates, Repositories, DbContext│
└─────────────────────────────────────────────────┘
         ┌────────┴────────┐
         │                 │
┌────────▼──────┐  ┌──────▼────────────┐
│ Common        │  │ CommonWebLib      │
│ (Helpers,     │  │ (Base Controllers,│
│  Auth Config) │  │  HTTP Clients)    │
└───────────────┘  └───────────────────┘
```

### Layer Responsibilities

- **Web**: HTTP request handling, API endpoints, request/response DTOs
- **Application**: Business logic, orchestration, use cases, caching
- **Domain**: Core business entities, domain logic, repository interfaces
- **Common**: Shared utilities, helpers, and cross-cutting concerns
- **CommonWebLib**: Reusable web infrastructure components

## 📝 Configuration

Key configuration sections in `appsettings.json`:

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

## 🔧 Adding a New Feature Module

Follow these steps to add a new business module:

1. **Define Domain Models** in `StandardWeb.Domain/Models/[YourModule]`
2. **Create Repository Interface** in `StandardWeb.Domain/Repositories/Interfaces`
3. **Implement Repository** in `StandardWeb.Domain/Repositories`
4. **Create Application Service** in `StandardWeb.Application/Services/[YourModule]`
5. **Define DTOs** in `StandardWeb.Web/Dtos/[YourModule]`
6. **Create Controller** in `StandardWeb.Web/Controllers/[YourModule]Controller.cs`
7. **Add AutoMapper Profile** in `StandardWeb.Web/Mapper/[YourModule]Profile.cs`
8. **Add Validators** in `StandardWeb.Web/Validators/[YourModule]` (if needed)

Example controller structure:
```csharp
[Route("api/[controller]")]
public class ProductController : BaseApiController
{
    private readonly IProductService _service;

    public ProductController(InfrastructureTools tools, IProductService service) 
        : base(tools)
    {
        ModuleCode = "03"; // Unique module code
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

## 🧪 Testing

The template is designed to be easily testable:

1. **Unit Tests**: Test domain logic and services in isolation
2. **Integration Tests**: Test API endpoints with in-memory database
3. **E2E Tests**: Test complete workflows through HTTP clients

Example test structure:
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

## 📚 Additional Resources

- **MiCake Framework**: [GitHub Repository](https://github.com/MiCake/MiCake)
- **ASP.NET Core**: [Official Documentation](https://docs.microsoft.com/aspnet/core)
- **Domain-Driven Design**: [DDD Reference](https://www.domainlanguage.com/ddd/)
- **Clean Architecture**: [Clean Architecture Guide](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 Contributing

Contributions are welcome! Please feel free to submit issues or pull requests to improve these templates.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 💬 Support

- **Issues**: [GitHub Issues](https://github.com/MiCake/MiCake.Templates/issues)
- **Discussions**: [GitHub Discussions](https://github.com/MiCake/MiCake.Templates/discussions)
- **MiCake Framework**: [MiCake Repository](https://github.com/MiCake/MiCake)

## 🎯 Roadmap

- [ ] Add more template variants (Microservices, Blazor, etc.)
- [ ] Include Docker and Docker Compose configurations
- [ ] Add example test projects
- [ ] Create migration guides from existing projects
- [ ] Add CI/CD pipeline templates (GitHub Actions, Azure DevOps)

---

**Happy Coding! 🎉**
