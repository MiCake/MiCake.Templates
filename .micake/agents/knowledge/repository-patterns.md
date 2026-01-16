# Repository Patterns in MiCake

Repository abstractions and usage patterns.

## Architecture

```
Abstraction Layer
  IRepository<TAggregateRoot, TKey>
  └── ICustomRepository (custom interface)

 Implementation Layer
  EFRepository<TAggregateRoot, TKey, TDbContext>
  └── CustomRepository (implementation)
```

## MiCake Provided Repository

### Interfaces
+ IRepository<TAggregateRoot, TKey>: Generic repository interface for aggregate roots, with basic operations.
+ IRepositoryHasPagingQuery<TAggregateRoot, TKey>: Extends IRepository with paging query support.

### Implementation
+ EFRepository<TAggregateRoot, TKey, TDbContext>: Base implementation using Entity Framework Core.
+ EFRepositoryHasPagingQuery<TAggregateRoot, TKey, TDbContext>: Extends EFRepository with paging query capabilities.

User can extend these base interfaces and classes to create custom repositories.

## Custom Repository

### Interface (Domain Layer)

```csharp
// Domain/Repositories/IOrderRepository.cs
public interface IOrderRepository : IRepository<Order, long>
{
    Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        long customerId, 
        CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default);
    
    Task<bool> ExistsActiveOrderForProductAsync(
        long productId, 
        CancellationToken cancellationToken = default);
}
```

### Implementation (Infrastructure Layer)

```csharp
// Infrastructure/Repositories/OrderRepository.cs
public class OrderRepository : EFRepository<Order, long, AppDbContext>, IOrderRepository
{
    public OrderRepository(AppDbContext dbContext) : base(dbContext) { }
    
    public async Task<IReadOnlyList<Order>> GetByCustomerIdAsync(
        long customerId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.CustomerId == customerId)
            .Include(o => o.Items)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<IReadOnlyList<Order>> GetPendingOrdersAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(o => o.Status == OrderStatus.Pending)
            .ToListAsync(cancellationToken);
    }
    
    public async Task<bool> ExistsActiveOrderForProductAsync(
        long productId, 
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(o => 
                o.Items.Any(i => i.ProductId == productId) && 
                o.Status != OrderStatus.Cancelled,
                cancellationToken);
    }
}
```

## Query Patterns

### Single Aggregate

```csharp
// Returns null if not found
var order = await _orderRepository.FindAsync(orderId);
```

### List Query

```csharp
public async Task<IReadOnlyList<Order>> GetByStatusAsync(
    OrderStatus status,
    CancellationToken cancellationToken = default)
{
    return await DbSet
        .Where(o => o.Status == status)
        .ToListAsync(cancellationToken);
}
```

## Best Practices

1. Repositories work only with aggregate roots
2. Use Include for loading related entities
3. Avoid returning IQueryable from repositories
4. Keep repository methods focused and specific
5. Use CancellationToken for async operations
