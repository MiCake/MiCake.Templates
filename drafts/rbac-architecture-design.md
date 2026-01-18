# RBAC Architecture Design Document

## Overview

This document describes a complete Role-Based Access Control (RBAC) architecture for the StandardWeb project. The design supports:
- **Role-based permission control**: Different roles can access specific resources
- **Data scope control**: Users can access data within their authorized scope (e.g., regional data)
- **Flexible resource management**: Define and manage system resources dynamically

## Design Goals

| Goal | Description |
|------|-------------|
| Flexibility | Support fine-grained permission control at operation and data levels |
| Scalability | Easy to extend new permission types and resources |
| Performance | Minimize authorization check overhead |
| Maintainability | Clear separation of concerns following DDD principles |
| Integration | Seamless integration with existing User management |

---

## Domain Model Design

### Bounded Context: Identity (Extended)

The RBAC domain extends the existing Identity bounded context with the following aggregates:

```
┌─────────────────────────────────────────────────────────────────────┐
│                        Identity Context                              │
├─────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐     ┌─────────────┐     ┌─────────────┐           │
│  │    User     │◄────│  UserRole   │────►│    Role     │           │
│  │ (Aggregate) │     │  (Entity)   │     │ (Aggregate) │           │
│  └─────────────┘     └─────────────┘     └──────┬──────┘           │
│                                                  │                  │
│                                          ┌──────▼──────┐           │
│                                          │RolePermission│           │
│                                          │  (Entity)    │           │
│                                          └──────┬──────┘           │
│                                                  │                  │
│  ┌─────────────┐     ┌─────────────┐     ┌──────▼──────┐           │
│  │  DataScope  │◄────│ RoleDataScope│    │ Permission  │           │
│  │ (Aggregate) │     │  (Entity)   │     │ (Aggregate) │           │
│  └─────────────┘     └─────────────┘     └──────┬──────┘           │
│                                                  │                  │
│                                          ┌──────▼──────┐           │
│                                          │  Resource   │           │
│                                          │ (Aggregate) │           │
│                                          └─────────────┘           │
└─────────────────────────────────────────────────────────────────────┘
```

---

## Aggregate Definitions

### 1. Role Aggregate

The Role aggregate is the core of RBAC, representing a named collection of permissions.

```yaml
aggregate: Role
root: Role
entities:
  - RolePermission
  - RoleDataScope
value_objects:
  - RoleCode
domain_events:
  - RoleCreatedEvent
  - RolePermissionChangedEvent
  - RoleDataScopeChangedEvent
```

#### Role (Aggregate Root)

| Property | Type | Description |
|----------|------|-------------|
| Id | long | Primary key |
| Code | string | Unique role code (e.g., "ADMIN", "REGIONAL_MANAGER") |
| Name | string | Display name |
| Description | string? | Role description |
| IsSystem | bool | Whether it's a system built-in role (cannot be deleted) |
| IsActive | bool | Whether the role is active |
| ParentRoleId | long? | Parent role for inheritance (optional) |

**Invariants:**
- Role code must be unique across the system
- System roles cannot be deleted or deactivated
- Child roles inherit parent permissions unless explicitly overridden

#### RolePermission (Entity)

| Property | Type | Description |
|----------|------|-------------|
| Id | long | Primary key |
| RoleId | long | Foreign key to Role |
| PermissionId | long | Foreign key to Permission |
| IsGranted | bool | True = granted, False = explicitly denied |

#### RoleDataScope (Entity)

| Property | Type | Description |
|----------|------|-------------|
| Id | long | Primary key |
| RoleId | long | Foreign key to Role |
| DataScopeId | long | Foreign key to DataScope |

---

### 2. Permission Aggregate

Represents an atomic permission that can be granted to roles.

```yaml
aggregate: Permission
root: Permission
entities: []
value_objects:
  - PermissionCode
domain_events:
  - PermissionCreatedEvent
```

#### Permission (Aggregate Root)

| Property | Type | Description |
|----------|------|-------------|
| Id | long | Primary key |
| Code | string | Unique permission code (e.g., "user:read", "user:write") |
| Name | string | Display name |
| Description | string? | Permission description |
| ResourceId | long | Associated resource |
| Action | PermissionAction | Action type (Read, Create, Update, Delete, Execute) |
| IsActive | bool | Whether permission is active |

**Permission Code Convention:**
- Format: `{resource}:{action}` or `{module}:{resource}:{action}`
- Examples: `user:read`, `user:write`, `system:settings:manage`

---

### 3. Resource Aggregate

Represents a protected system resource (module, API endpoint, data entity).

```yaml
aggregate: Resource
root: Resource
entities: []
value_objects:
  - ResourceType
domain_events:
  - ResourceCreatedEvent
```

#### Resource (Aggregate Root)

| Property | Type | Description |
|----------|------|-------------|
| Id | long | Primary key |
| Code | string | Unique resource code (e.g., "module:user-management") |
| Name | string | Display name |
| Description | string? | Resource description |
| Type | ResourceType | Type of resource (Menu, API, Data, Button) |
| ParentId | long? | Parent resource for hierarchy |
| Path | string? | Resource path (URL for API, route for Menu) |
| SortOrder | int | Display order |
| IsActive | bool | Whether resource is active |

---

### 4. DataScope Aggregate

Defines data access boundaries for data-level permission control.

```yaml
aggregate: DataScope
root: DataScope
entities: []
value_objects:
  - DataScopeType
  - ScopeCondition
domain_events:
  - DataScopeCreatedEvent
```

#### DataScope (Aggregate Root)

| Property | Type | Description |
|----------|------|-------------|
| Id | long | Primary key |
| Code | string | Unique scope code (e.g., "region:east", "dept:sales") |
| Name | string | Display name |
| Description | string? | Scope description |
| Type | DataScopeType | Type of scope (All, Region, Department, Self, Custom) |
| Condition | string? | Custom filter condition (JSON/SQL expression) |
| Priority | int | Priority when multiple scopes apply |
| IsActive | bool | Whether scope is active |

**DataScopeType Values:**

| Type | Description |
|------|-------------|
| All | Access all data |
| Department | Access department data only |
| DepartmentAndBelow | Access department and subordinate departments |
| Region | Access regional data only |
| Self | Access own data only |
| Custom | Custom condition-based scope |

---

### 5. UserRole (Join Entity - under User Aggregate)

Links users to their assigned roles.

| Property | Type | Description |
|----------|------|-------------|
| Id | long | Primary key |
| UserId | long | Foreign key to User |
| RoleId | long | Foreign key to Role |
| AssignedAt | DateTime | When the role was assigned |
| ExpiresAt | DateTime? | Optional expiration date |
| IsActive | bool | Whether assignment is active |

---

## Enum Definitions

### PermissionAction

```csharp
public enum PermissionAction
{
    Read = 1,      // View/List
    Create = 2,    // Create new
    Update = 3,    // Modify existing
    Delete = 4,    // Remove
    Execute = 5,   // Execute operation
    Manage = 6,    // Full management
    Export = 7,    // Export data
    Import = 8     // Import data
}
```

### ResourceType

```csharp
public enum ResourceType
{
    Module = 1,    // System module
    Menu = 2,      // Navigation menu
    API = 3,       // API endpoint
    Button = 4,    // UI button/action
    Data = 5       // Data entity
}
```

### DataScopeType

```csharp
public enum DataScopeType
{
    All = 1,               // Access all data
    Department = 2,         // Own department only
    DepartmentAndBelow = 3, // Department hierarchy
    Region = 4,             // Regional boundary
    Self = 5,               // Only own records
    Custom = 6              // Custom expression
}
```

---

## Domain Services

### PermissionChecker Service

Responsible for checking if a user has specific permissions.

```
IPermissionChecker
├── HasPermissionAsync(userId, permissionCode) -> bool
├── GetUserPermissionsAsync(userId) -> IEnumerable<Permission>
├── GetUserRolesAsync(userId) -> IEnumerable<Role>
└── IsInRoleAsync(userId, roleCode) -> bool
```

### DataScopeResolver Service

Responsible for resolving data scope filters for queries.

```
IDataScopeResolver
├── GetDataScopeAsync(userId, resourceCode) -> DataScopeFilter
├── ApplyDataScopeAsync<T>(query, userId) -> IQueryable<T>
└── GetAccessibleScopesAsync(userId) -> IEnumerable<DataScope>
```

---

## Authorization Flow

### Permission Check Flow

```
┌─────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Request   │───►│ Authorization    │───►│  Permission     │
│             │    │ Middleware       │    │  Checker        │
└─────────────┘    └────────┬─────────┘    └────────┬────────┘
                            │                       │
                            │                       ▼
                            │              ┌─────────────────┐
                            │              │  User Roles     │
                            │              │  (Cached)       │
                            │              └────────┬────────┘
                            │                       │
                            │                       ▼
                            │              ┌─────────────────┐
                            │              │ Role Permissions│
                            │              │  (Cached)       │
                            │              └────────┬────────┘
                            ▼                       │
                   ┌─────────────────┐              │
                   │ Allow / Deny    │◄─────────────┘
                   └─────────────────┘
```

### Data Scope Application Flow

```
┌─────────────┐    ┌──────────────────┐    ┌─────────────────┐
│   Query     │───►│ DataScope        │───►│  DataScope      │
│   Request   │    │ Interceptor      │    │  Resolver       │
└─────────────┘    └────────┬─────────┘    └────────┬────────┘
                            │                       │
                            ▼                       ▼
                   ┌─────────────────┐     ┌─────────────────┐
                   │ Apply Filter    │◄────│ Scope Condition │
                   │ to Query        │     │                 │
                   └────────┬────────┘     └─────────────────┘
                            │
                            ▼
                   ┌─────────────────┐
                   │ Filtered Result │
                   └─────────────────┘
```

---

## Project Structure

### Domain Layer Changes

```
StandardWeb.Domain/
├── Models/
│   └── Identity/
│       ├── User.cs                    # Extend with UserRoles
│       ├── UserRole.cs                # NEW
│       └── ...
│   └── Authorization/                  # NEW folder
│       ├── Role.cs                    # NEW
│       ├── RolePermission.cs          # NEW
│       ├── RoleDataScope.cs           # NEW
│       ├── Permission.cs              # NEW
│       ├── Resource.cs                # NEW
│       └── DataScope.cs               # NEW
├── Enums/
│   └── Authorization/                  # NEW folder
│       ├── PermissionAction.cs        # NEW
│       ├── ResourceType.cs            # NEW
│       └── DataScopeType.cs           # NEW
└── Repositories/
    ├── IRoleRepo.cs                   # NEW
    ├── IPermissionRepo.cs             # NEW
    ├── IResourceRepo.cs               # NEW
    └── IDataScopeRepo.cs              # NEW
```

### Application Layer Changes

```
StandardWeb.Application/
├── Services/
│   └── Authorization/                  # NEW folder
│       ├── RoleService.cs             # NEW
│       ├── PermissionService.cs       # NEW
│       ├── ResourceService.cs         # NEW
│       └── DataScopeService.cs        # NEW
├── Authorization/                      # NEW folder
│   ├── IPermissionChecker.cs          # NEW
│   ├── PermissionChecker.cs           # NEW
│   ├── IDataScopeResolver.cs          # NEW
│   ├── DataScopeResolver.cs           # NEW
│   └── PermissionCache.cs             # NEW
└── Audit/
    └── ICurrentUser.cs                # Extend with role/permission methods
```

### Contracts Layer Changes

```
StandardWeb.Contracts/
└── Dtos/
    └── Authorization/                  # NEW folder
        ├── RoleDto.cs                 # NEW
        ├── PermissionDto.cs           # NEW
        ├── ResourceDto.cs             # NEW
        ├── DataScopeDto.cs            # NEW
        ├── UserRoleAssignmentDto.cs   # NEW
        └── PermissionCheckResultDto.cs # NEW
```

### Web Layer Changes

```
StandardWeb.Web/
├── Controllers/
│   ├── RoleController.cs              # NEW
│   ├── PermissionController.cs        # NEW
│   └── ResourceController.cs          # NEW
├── Authorization/                      # NEW folder
│   ├── PermissionAuthorizationHandler.cs    # NEW
│   ├── PermissionRequirement.cs             # NEW
│   ├── DataScopeAuthorizationHandler.cs     # NEW
│   └── PermissionAttribute.cs               # NEW
└── StartUp/
    └── AuthorizationConfiguration.cs  # NEW
```

### EFCore Layer Changes

```
StandardWeb.EFCore/
├── AppDbContext.cs                    # Add new DbSets
└── Repositories/
    ├── RoleRepo.cs                    # NEW
    ├── PermissionRepo.cs              # NEW
    ├── ResourceRepo.cs                # NEW
    └── DataScopeRepo.cs               # NEW
```

---

## API Endpoints Design

### Role Management

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | /api/roles | role:read | List all roles |
| GET | /api/roles/{id} | role:read | Get role details |
| POST | /api/roles | role:create | Create new role |
| PUT | /api/roles/{id} | role:update | Update role |
| DELETE | /api/roles/{id} | role:delete | Delete role |
| POST | /api/roles/{id}/permissions | role:manage | Assign permissions |
| DELETE | /api/roles/{id}/permissions/{permissionId} | role:manage | Remove permission |
| POST | /api/roles/{id}/datascopes | role:manage | Assign data scope |

### Permission Management

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | /api/permissions | permission:read | List all permissions |
| GET | /api/permissions/{id} | permission:read | Get permission details |
| POST | /api/permissions | permission:create | Create new permission |
| PUT | /api/permissions/{id} | permission:update | Update permission |

### User Role Assignment

| Method | Endpoint | Permission | Description |
|--------|----------|------------|-------------|
| GET | /api/users/{id}/roles | user:read | Get user roles |
| POST | /api/users/{id}/roles | user:manage | Assign role to user |
| DELETE | /api/users/{id}/roles/{roleId} | user:manage | Remove role from user |
| GET | /api/users/{id}/permissions | user:read | Get user effective permissions |

---

## Caching Strategy

### Permission Cache Structure

```
Cache Key Pattern: "user:{userId}:permissions"
Cache Duration: 5 minutes (configurable)
Invalidation: On role assignment change, permission change
```

### Role Cache Structure

```
Cache Key Pattern: "role:{roleId}:permissions"
Cache Duration: 10 minutes (configurable)
Invalidation: On role permission change
```

### Cache Invalidation Events

| Event | Invalidated Caches |
|-------|-------------------|
| RolePermissionChanged | role:{roleId}:*, user:*:permissions (for affected users) |
| UserRoleChanged | user:{userId}:* |
| RoleDeleted | role:{roleId}:*, user:*:permissions |

---

## Seed Data

### Default Roles

| Code | Name | IsSystem | Description |
|------|------|----------|-------------|
| SUPER_ADMIN | Super Administrator | true | Full system access |
| ADMIN | Administrator | true | System management access |
| REGIONAL_MANAGER | Regional Manager | false | Regional data access |
| USER | Standard User | true | Basic user access |

### Default Permissions

| Code | Resource | Action | Description |
|------|----------|--------|-------------|
| user:read | user-management | Read | View users |
| user:create | user-management | Create | Create users |
| user:update | user-management | Update | Modify users |
| user:delete | user-management | Delete | Delete users |
| role:read | role-management | Read | View roles |
| role:manage | role-management | Manage | Full role management |
| system:settings:read | system-settings | Read | View settings |
| system:settings:manage | system-settings | Manage | Manage settings |

### Default Data Scopes

| Code | Name | Type | Description |
|------|------|------|-------------|
| scope:all | All Data | All | Access all system data |
| scope:self | Own Data | Self | Access only own records |
| scope:department | Department Data | Department | Access department data |

---

## Integration Points

### JWT Token Design

The JWT token should remain lightweight. Only essential identity information is stored in the token; permissions and data scopes are resolved at runtime via cache.

#### Token Payload (Minimal)

```json
{
  "userid": "123",
  "phonenumber": "13800138000",
  "roles": ["1", "3"]
}
```

#### Design Decisions

| Item | Store in Token? | Rationale |
|------|-----------------|-----------|
| User ID | ✅ Yes | Essential for identity, small size |
| Phone Number | ✅ Yes | Common claim, small size |
| Role IDs | ✅ Yes | Use IDs (not names) - immutable, smaller, name changes don't invalidate tokens |
| Role Names | ❌ No | Names can change, wastes space |
| Permissions | ❌ No | Large volume, resolve via cache using role IDs |
| Data Scopes | ❌ No | Large volume, resolve via cache using role IDs |

#### Why Role IDs over Role Names?

| Aspect | Role ID | Role Name |
|--------|---------|-----------|
| Token Size | Smaller (e.g., "1") | Larger (e.g., "REGIONAL_MANAGER") |
| Immutability | IDs never change | Names may be renamed |
| Token Validity | Remains valid after rename | Invalidated if role renamed |
| Lookup | Direct cache key | Requires name-to-id mapping |

**Recommendation**: Store Role IDs in JWT. The authorization flow uses these IDs as cache keys to fetch permissions and data scopes.

#### Runtime Authorization Flow

```
Request → Extract userId + roleIds from JWT
                    ↓
        Cache Lookup: "roles:{roleId}:permissions"
                    ↓
        Aggregate all permissions from user's roles
                    ↓
        Check if required permission exists
                    ↓
        Cache Lookup: "roles:{roleId}:datascopes" (if needed)
                    ↓
        Apply data scope filter to query
```

### ICurrentUser Extension

Extend the existing `ICurrentUser` interface:

```csharp
public interface ICurrentUser
{
    long? GetCurrentUserId();
    IEnumerable<long> GetRoleIds();           // NEW - from JWT claims
    
    // These methods fetch from cache, not JWT
    Task<IEnumerable<string>> GetRolesAsync();           // NEW
    Task<IEnumerable<string>> GetPermissionsAsync();     // NEW
    Task<bool> HasPermissionAsync(string permission);    // NEW
    Task<bool> IsInRoleAsync(string roleCode);           // NEW
    Task<DataScopeFilter?> GetDataScopeAsync();          // NEW
}
```

**Note**: Methods that access permissions and data scopes are async because they may hit the cache (or database on cache miss).

---

## Trade-offs Considered

| Approach | Pros | Cons | Decision |
|----------|------|------|----------|
| Store permissions in JWT | Fast authorization, no DB hit | Token size, cannot revoke immediately | Use for basic claims, cache complex permissions |
| Fine-grained permissions | Maximum flexibility | Complex management | ✅ Recommended for enterprise apps |
| Coarse-grained roles only | Simple implementation | Limited flexibility | Not recommended |
| Permission inheritance | Reduces duplication | Complex hierarchy resolution | ✅ Optional feature via ParentRoleId |
| Custom data scope expressions | Maximum flexibility | Security risk, complex parsing | ✅ Recommended with validation |

---
