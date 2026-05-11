# MiCake.Templates
简体中文 | [English](README.md)

基于 [MiCake 框架](https://github.com/MiCake/MiCake)的生产就绪项目模板集合，旨在帮助开发者快速搭建遵循领域驱动设计（DDD）原则的高质量 ASP.NET Core 应用程序。

## 🌟 特性

- **生产就绪**：基于最佳实践和真实场景构建
- **DDD 架构**：清晰的领域层、应用层和基础设施层分离
- **开发体验**：集成各种开箱即用的开发工具和配置，提升开发效率
- **可扩展性**：模块化设计使得添加新功能和自定义变得简单

## 📦 可用模板
| 模板名称               | 描述                                         |
| ---------------------- | -------------------------------------------- |
| MiCake WebAPI Template | 基于 MiCake 的标准 ASP.NET Core Web API 模板 |
| MiCake WebAPI Template with RBAC | 基于 MiCake 的内置 RBAC 模块 ASP.NET Core Web API 模板 |

## 🚀 快速开始

1. **安装模板集合**
```powershell
dotnet new install MiCake.Templates
```

2. **创建新项目**
根据场景选择一个模板：
```powershell
# 标准 WebAPI 模板
dotnet new micake-webapi -n YourProjectName

# 内置 RBAC 的 WebAPI 模板
dotnet new micake-webapi-rbac -n YourProjectName
```

3. **进入项目目录并运行**
```powershell
cd YourProjectName
dotnet build
dotnet run
```

## 📚 其他资源

- **MiCake 框架**：[GitHub 仓库](https://github.com/MiCake/MiCake)
- **领域驱动设计**：[DDD 参考](https://www.domainlanguage.com/ddd/)
- **整洁架构**：[整洁架构指南](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 📄 许可证

本项目采用 MIT 许可证 - 详见 [LICENSE](LICENSE) 文件。

## 💬 支持

- **问题**：[GitHub Issues](https://github.com/MiCake/MiCake.Templates/issues)
- **讨论**：[GitHub Discussions](https://github.com/MiCake/MiCake.Templates/discussions)
- **MiCake 框架**：[MiCake 仓库](https://github.com/MiCake/MiCake)

---

**编码愉快！🎉**
