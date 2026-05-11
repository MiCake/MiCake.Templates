# RBAC 权限控制系统开发指南

本文档旨在帮助开发者快速理解并使用该框架的 RBAC（Role-Based Access Control）权限控制系统。

该系统提供了灵活的权限管理机制，支持基于 **功能（Permission）** 的访问控制和基于 **数据范围（DataScope）** 的数据隔离。

## 1. 核心概念 (Core Concepts)

RBAC 系统由以下五个核心维度组成，它们共同决定了 "谁（User）" 可以对 "什么资源（Resource）" 进行 "什么操作（Permission）"，以及能看到 "哪些数据（DataScope）"。

### 1.1 用户 (User)
- 系统的操作主体。
- 用户通过拥有 **角色 (Role)** 来间接获取权限和数据范围。
- 用户可以拥有多个角色。

### 1.2 角色 (Role)
- 权限和数据范围的集合容器。
- **变更点**：角色不再包含 `Code`（唯一编码），仅作为 ID 或名称标识的逻辑分组。
- 作用：简化对大量用户的权限分配。将一组 `Permission` 和 `DataScope` 关联到一个 Role，再将 Role 分配给 User。

### 1.3 权限 (Permission)
- 访问控制的最小单元。
- **关键属性**：
    - `Code` (唯一编码): 例如 `user:create`, `order:view`。代码中通过此编码进行检查。
    - `ResourceId` (可选): 权限可以关联到一个资源，也可以是独立的（系统级权限）。
- **变更点**：`ResourceId` 现在是可空的 (`Nullable`)。这意味着你可以定义不属于任何具体资源的全局权限（如 "System:Login"）。

### 1.4 资源 (Resource)
- 系统中的实体或功能模块（如 "用户管理", "订单服务"）。
- 主要用于在 UI 上对权限进行分类展示，辅助管理。
- 在代码逻辑中，权限检查主要依赖 `Permission.Code`，而非资源。

### 1.5 数据范围 (DataScope)
- 决定用户在拥有 "查看" 权限时，具体能看到 **多少** 数据。
- 常见类型：
    - **All (全部)**: 能看到所有数据。
    - **Department (部门)**: 只能看到本部门数据。
    - **Self (仅本人)**: 只能看到自己创建的数据。
    - **Custom (自定义)**: 基于特定规则过滤。

---

## 2. 开发实战指南

### 2.1 依赖注入与服务

在你的 Application 层或 Web 层中，你可以通过 `ICurrentUser` 接口获取当前用户的权限状态。

```csharp
public class MyService : IMyService
{
    private readonly ICurrentUser _currentUser;
    private readonly IRepository<Order> _orderRepo;

    public MyService(ICurrentUser currentUser, IRepository<Order> orderRepo)
    {
        _currentUser = currentUser;
        _orderRepo = orderRepo;
    }
}
```

### 2.2 功能权限控制 (Functional Authorization)

主要用于控制 **API 接口的访问** 或 **UI 按钮的显示**。

#### 场景 A: 保护 API 接口 (推荐)

使用 `[RequirePermission]` 特性标记 Controller 或 Action。只有拥有对应权限编码的用户才能访问。

```csharp
using RBACWeb.Web.Authorization;

[ApiController]
[Route("api/[controller]")]
// 1. 整个 Controller 需要权限
[RequirePermission("order:management")] 
public class OrderController : ControllerBase
{
    [HttpGet]
    // 2. 特定接口需要更细粒度的权限
    [RequirePermission("order:view")] 
    public async Task<IActionResult> GetOrders()
    {
        // ...
    }

    [HttpPost]
    [RequirePermission("order:create")]
    public async Task<IActionResult> CreateOrder()
    {
        // ...
    }
}
```

> **注意**: 系统不再提供 `[RequireRole]` 特性。所有鉴权应基于 Permission（做什么），而不是 Role（是谁）。这使得权限系统更灵活，代码与具体角色解耦。

#### 场景 B: 代码逻辑中的手动检查

有时你需要在业务逻辑内部判断权限（例如：根据权限决定是否执行某个分支逻辑）。

```csharp
public async Task DoSomethingSpecific()
{
    // 检查当前用户是否拥有 "system:admin_action" 权限
    if (await _currentUser.HasPermissionAsync("system:admin_action"))
    {
        // 执行管理员特权逻辑
    }
}
```

---

### 2.3 数据权限控制 (Data Authorization)

主要用于控制 **查询返回的数据行数**。

#### 场景 C: 自动数据过滤

当用户通过 `ICurrentUser` 获取 DataScope 时，系统会计算该用户所有角色中 **最宽松** 的数据范围。

```csharp
public async Task<List<OrderDto>> GetMyOrdersAsync()
{
    // 1. 获取当前用户的数据范围过滤器
    var dataScope = await _currentUser.GetDataScopeAsync();

    // 2. 构建查询
    IQueryable<Order> query = _orderRepo.GetQueryable();

    // 3. 应用过滤 (示例逻辑)
    // 如果是 "All"，则不过滤
    // 如果是 "Self"，则过滤 CreatorId == CurrentUserId
    // 如果是 "Department"，则过滤 OrgId == CurrentUserOrgId
    
    if (dataScope.Type == DataScopeType.Self)
    {
        var userId = _currentUser.GetCurrentUserId();
        query = query.Where(o => o.CreatorId == userId);
    }
    else if (dataScope.Type == DataScopeType.Department)
    {
        // 假设 CurrentUser store claims 包含 DeptId
        // query = query.Where(...) 
    }
    
    // ... 执行查询
    return await query.ToListAsync();
}
```

---

## 3. 组合模式详解 (Combination Patterns)

本章节详细说明 **角色(Role)**、**权限(Permission)**、**资源(Resource)**、**数据范围(DataScope)** 如何在不同业务场景下进行组合使用。

### 3.1 模式一：角色 + 权限 (Role + Permission)

> **核心逻辑**: "这个人能不能做这件事？"

这是最基础的形态。此时 Permission 通常不关联任何 Resource (ResourceId 为空)，仅代表一个全局的行为能力。

*   **适用场景**: 
    *   **系统功能开关**: 如 "登录系统"、"查看仪表盘"、"切换维护模式"。
    *   **公共服务**: 如 "文件上传"、"公共字典查询"。
*   **配置示例**:
    1.  **Permission**: 创建 `Code="system:upload"`, `ResourceId=null`。
    2.  **Role**: 创建 "普通用户" 角色，关联 `system:upload`。
    3.  **User**: 张三拥有 "普通用户" 角色。
*   **实际效果**:
    *   代码中使用 `[RequirePermission("system:upload")]` 保护上传接口。
    *   张三可以调用该接口，但他传什么文件、传多少文件，此模式不关心。

---

### 3.2 模式二：角色 + 权限 + 资源 (Role + Permission + Resource)

> **核心逻辑**: "这个人能不能对**这个模块**做这件事？"

这是标准的后台管理系统形态。Resource 在这里主要扮演 **"容器"** 和 **"导航锚点"** 的角色。

*   **适用场景**: 
    *   **模块化管理**: 用户管理、订单管理、CMS 内容发布。
    *   **菜单生成**: 前端通常需要根据 Resource 结构来渲染左侧菜单树。
*   **配置示例**:
    1.  **Resource**: 创建 `Code="res:order"`, `Name="订单管理"`。
    2.  **Permission**: 
        *   创建 `Code="order:view"`, 关联 `res:order`。
        *   创建 `Code="order:edit"`, 关联 `res:order`。
    3.  **Role**: 
        *   "客服": 关联 `order:view`。
        *   "运营": 关联 `order:view` + `order:edit`。
*   **实际效果**:
    *   **后端**: 接口 `[RequirePermission("order:edit")]` 会拦截"客服"，放行"运营"。
    *   **前端**: 登录后获取权限树，发现 "运营" 拥有 `res:order` 下的两个权限，因此显示 "订单管理" 菜单及 "编辑" 按钮；而 "客服" 只显示菜单，不显示 "编辑" 按钮。

---

### 3.3 模式三：角色 + 权限 + 资源 + DataScope (Role + Permission + Resource + DataScope)

> **核心逻辑**: "这个人能对这个模块做这件事，但他**只能看到这些数据**。"

这是企业级应用中最复杂也最强大的形态。它解决了 "职位决定权力，但也决定视野" 的问题。

*   **适用场景**: 
    *   **销售体系**: 销售员看自己，经理看部门，总监看全公司。
    *   **地域隔离**: 华东区负责人只能管理华东区的业务。
*   **配置示例**:
    1.  **基础建设**: 设定 Resource (`res:contract`) 和 Permission (`contract:view`)。
    2.  **DataScope**: 
        *   `DS_Self`: Type=Self (仅本人)。
        *   `DS_Dept`: Type=Department (本部门)。
    3.  **Role 组合**:
        *   **角色 A (普通销售)**: 关联 `contract:view` + `DS_Self`。
        *   **角色 B (销售经理)**: 关联 `contract:view` + `DS_Dept`。
*   **实际过程与效果**:
    1.  **鉴权阶段 (进入)**: 
        *   无论是普通销售还是经理，访问 `GET /api/contracts` 时，因都有 `contract:view` 权限，**均通过 Web层拦截器**。
    2.  **数据获取阶段 (查询)**:
        *   Service 层调用 `_currentUser.GetDataScopeAsync()`。
        *   **普通销售** 得到的 Scope 是 `Self` -> 代码拼接 SQL `WHERE CreatorId = {MyId}` -> 返回 5 条数据。
        *   **销售经理** 得到的 Scope 是 `Department` -> 代码拼接 SQL `WHERE DeptId = {MyDeptId}` -> 返回 50 条数据。

#### 扩展：多角色合并 (Merging)
如果一个用户同时是 "销售经理" (看部门) 又是 "特殊项目组员" (能看所有)，系统会取 **并集 (Union)** 或 **最宽松 (Most Permissive)** 的范围，最终让他看到 "所有数据"。

---

## 4. 最佳实践 (Best Practices)

1.  **始终基于 Permission 鉴权**: 不要编写 `if (role == "admin")` 这样的代码。使用 `RequirePermission` 或 Policy，这样当你需要给新角色（如 "SuperUser"）赋予同样权限时，无需修改代码。
2.  **权限编码规范**: 使用 `resource:action` 格式（如 `product:edit`, `report:export`），保持语义清晰。
3.  **最小权限原则**: 给 Role 分配 DataScope 时，优先分配最小适用范围（如先给 Self，确有需要再给 All）。
4.  **资源解耦**: 既然 Permission 的 Resource 是可选的，对于通用的、非业务实体的操作，尽量创建不依赖 Resource 的独立 Permission，减少维护成本。
