# RBAC Authorization System Development Guide

This document aims to help developers quickly understand and use the framework's RBAC (Role-Based Access Control) authorization system.

The system provides flexible permission management mechanisms, supporting function-based (Permission) access control and data-range-based (DataScope) data isolation.

## 1. Core Concepts

The RBAC system consists of the following five core dimensions; together they determine "who (User)" can perform "what operation (Permission)" on "which resource (Resource)" and what "data (DataScope)" they can see.

### 1.1 User
- The actor of the system.
- A user obtains permissions and data scopes indirectly by possessing **roles (Role)**.
- A user can have multiple roles.

### 1.2 Role
- A container that groups permissions and data scopes.
- Change: Roles no longer include a `Code` (unique code); they are only a logical grouping identified by ID or name.
- Purpose: Simplify permission assignments for many users. Associate a set of `Permission` and `DataScope` with a Role, then assign the Role to Users.

### 1.3 Permission
- The smallest unit of access control.
- Key properties:
  - `Code` (unique identifier): e.g., `user:create`, `order:view`. Checks in code are performed using this code.
  - `ResourceId` (optional): A permission may be associated with a resource, or it may be standalone (system-level permission).
- Change: `ResourceId` is now nullable. This means you can define global permissions that do not belong to any specific resource (e.g., "System:Login").

### 1.4 Resource
- Entities or functional modules in the system (such as "User Management", "Order Service").
- Mainly used to categorize permissions in the UI and assist management.
- In code logic, permission checks primarily rely on `Permission.Code`, not the resource.

### 1.5 DataScope
- Determines how much data a user can see when they have "view" permission.
- Common types:
  - All: can see all data.
  - Department: can see only data from their department.
  - Self: can see only data they created.
  - Custom: filtered by specific rules.

---

## 2. Practical Development Guide

### 2.1 Dependency Injection and Services

In your Application or Web layer, you can obtain the current user's permission state via the `ICurrentUser` interface.

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

### 2.2 Functional Authorization

Mainly used to control API access or the display of UI buttons.

#### Scenario A: Protecting API Endpoints (Recommended)

Mark Controllers or Actions with the `[RequirePermission]` attribute. Only users with the corresponding permission code can access them.

```csharp
using RBACWeb.Web.Authorization;

[ApiController]
[Route("api/[controller]")]
// 1. Require permission for the entire Controller
[RequirePermission("order:management")] 
public class OrderController : ControllerBase
{
    [HttpGet]
    // 2. Specific endpoint requires a more granular permission
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

> Note: The system no longer provides a `[RequireRole]` attribute. All authorization should be based on Permission (what action), not Role (who). This makes the permission system more flexible and decouples code from specific roles.

#### Scenario B: Manual Checks in Business Logic

Sometimes you need to check permissions inside business logic (for example, to decide whether to take a particular branch).

```csharp
public async Task DoSomethingSpecific()
{
    // Check whether the current user has the "system:admin_action" permission
    if (await _currentUser.HasPermissionAsync("system:admin_action"))
    {
        // Execute admin privileged logic
    }
}
```

---

### 2.3 Data Authorization

Mainly used to control the number of rows returned in queries.

#### Scenario C: Automatic Data Filtering

When a user retrieves a DataScope via `ICurrentUser`, the system computes the user's effective data range based on all their roles, choosing the most permissive scope.

```csharp
public async Task<List<OrderDto>> GetMyOrdersAsync()
{
    // 1. Get the current user's data scope filter
    var dataScope = await _currentUser.GetDataScopeAsync();

    // 2. Build the query
    IQueryable<Order> query = _orderRepo.GetQueryable();

    // 3. Apply filtering (example logic)
    // If "All", do not filter
    // If "Self", filter CreatorId == CurrentUserId
    // If "Department", filter OrgId == CurrentUserOrgId

    if (dataScope.Type == DataScopeType.Self)
    {
        var userId = _currentUser.GetCurrentUserId();
        query = query.Where(o => o.CreatorId == userId);
    }
    else if (dataScope.Type == DataScopeType.Department)
    {
        // Assume CurrentUser stores claims that include DeptId
        // query = query.Where(...) 
    }
    
    // ... execute query
    return await query.ToListAsync();
}
```

---

## 3. Combination Patterns Explained

This chapter details how Role, Permission, Resource, and DataScope combine in different business scenarios.

### 3.1 Pattern 1: Role + Permission

Core logic: "Can this person do this?"

This is the most basic form. In this pattern, Permission typically has no associated Resource (ResourceId is null) and represents a global capability.

- Applicable scenarios:
  - System feature toggles (e.g., "login to system", "view dashboard", "toggle maintenance mode").
  - Public services (e.g., "file upload", "public dictionary query").
- Configuration example:
  1. Permission: create `Code="system:upload"`, `ResourceId=null`.
  2. Role: create a "Regular User" role and associate `system:upload`.
  3. User: Zhang San is assigned the "Regular User" role.
- Actual effect:
  - In code use `[RequirePermission("system:upload")]` to protect the upload endpoint.
  - Zhang San can call the endpoint; how many or what files he uploads is outside this pattern's concern.

---

### 3.2 Pattern 2: Role + Permission + Resource

Core logic: "Can this person perform this action on this module?"

This is the standard structure for admin systems. Resource mainly acts as a container and navigation anchor.

- Applicable scenarios:
  - Modular management: user management, order management, CMS content publishing.
  - Menu generation: frontends usually render a left-side menu tree based on Resources.
- Configuration example:
  1. Resource: create `Code="res:order"`, `Name="Order Management"`.
  2. Permission:
     - Create `Code="order:view"`, associated with `res:order`.
     - Create `Code="order:edit"`, associated with `res:order`.
  3. Role:
     - "Customer Service": associated with `order:view`.
     - "Operations": associated with `order:view` + `order:edit`.
- Actual effect:
  - Backend: the `[RequirePermission("order:edit")]` attribute will block "Customer Service" and allow "Operations".
  - Frontend: after login, retrieve the permission tree; if "Operations" has the two permissions under `res:order`, show the "Order Management" menu and the "Edit" button; "Customer Service" sees the menu but not the "Edit" button.

---

### 3.3 Pattern 3: Role + Permission + Resource + DataScope

Core logic: "This person can perform this action on this module, but they can only see certain data."

This is the most complex and powerful form in enterprise applications. It solves the issue that "position determines capability, and also determines visibility."

- Applicable scenarios:
  - Sales systems: salespeople see their own data, managers see departmental data, directors see company-wide data.
  - Regional separation: a regional manager can only manage business within their region.
- Configuration example:
  1. Infrastructure: define Resource (`res:contract`) and Permission (`contract:view`).
  2. DataScope:
     - `DS_Self`: Type=Self (only self).
     - `DS_Dept`: Type=Department (department).
  3. Role composition:
     - Role A (Regular Sales): associated with `contract:view` + `DS_Self`.
     - Role B (Sales Manager): associated with `contract:view` + `DS_Dept`.
- Process and effect:
  1. Authorization stage (entry):
     - Both regular sales and managers pass the Web-layer interceptor for `GET /api/contracts` because they have `contract:view`.
  2. Data retrieval stage (query):
     - The Service layer calls `_currentUser.GetDataScopeAsync()`.
     - A regular salesperson receives the `Self` scope -> code appends SQL `WHERE CreatorId = {MyId}` -> returns 5 rows.
     - A sales manager receives the `Department` scope -> code appends SQL `WHERE DeptId = {MyDeptId}` -> returns 50 rows.

#### Extension: Merging Multiple Roles
If a user is both "Sales Manager" (department-level view) and "Special Project Member" (can see all), the system takes the union or the most permissive range, ultimately allowing them to see "all data."

---

## 4. Best Practices

- Always authorize based on Permission: avoid writing code like `if (role == "admin")`. Use `RequirePermission` or policy-based checks so that when you need to grant the same capability to a new role (e.g., "SuperUser"), you don't need to change code.
- Permission coding convention: use `resource:action` format (e.g., `product:edit`, `report:export`) for clear semantics.
- Principle of least privilege: when assigning DataScope to Roles, prefer the minimal applicable scope first (e.g., start with Self; grant All only when necessary).
- Decouple from Resources: since Permission's Resource is optional, for general or non-business-entity operations, create permissions that do not depend on a Resource to reduce maintenance overhead.
