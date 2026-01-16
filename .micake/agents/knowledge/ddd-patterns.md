# DDD Patterns in MiCake

Core Domain-Driven Design patterns implemented in the MiCake framework.

## Entity

Entities are domain objects with unique identity that persists throughout their lifecycle.

### Implementation

```csharp
using MiCake.DDD.Domain;

public class OrderItem : Entity<int>
{
    public int ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    
    private OrderItem() { }
    
    public OrderItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        ArgumentNullException.ThrowIfNull(productName);
        if (unitPrice < 0) throw new ArgumentException("Unit price cannot be negative");
        if (quantity <= 0) throw new ArgumentException("Quantity must be positive");
        
        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
    }
    
    public decimal GetTotalPrice() => UnitPrice * Quantity;
}
```

### Key Characteristics

- Inherits from `Entity<TKey>` or `Entity` (default int key)
- Identified by `Id` property
- Use private setters to protect state
- Validate invariants in constructor

## Aggregate Root

Aggregate roots define consistency boundaries. External objects access aggregate internals only through the root.

### Implementation

```csharp
using MiCake.DDD.Domain;

public class Order : AggregateRoot<long>
{
    private readonly List<OrderItem> _items = new();
    
    public long CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();
    
    private Order() { }
    
    public static Order Create(long customerId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(customerId);
        
        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };
        
        order.AddDomainEvent(new OrderCreatedEvent(order.Id, customerId));
        return order;
    }
    
    public void AddItem(int productId, string productName, decimal unitPrice, int quantity)
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot add items to a non-pending order");
            
        var item = new OrderItem(productId, productName, unitPrice, quantity);
        _items.Add(item);
        
        AddDomainEvent(new OrderItemAddedEvent(Id, productId, quantity));
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Only pending orders can be confirmed");
        if (!_items.Any())
            throw new InvalidOperationException("Cannot confirm an empty order");
            
        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id));
    }
    
    public decimal GetTotalAmount() => _items.Sum(i => i.GetTotalPrice());
}

public enum OrderStatus
{
    Pending,
    Confirmed,
    Shipped,
    Completed,
    Cancelled
}
```

### Key Characteristics

- Inherits from `AggregateRoot<TKey>`
- Only entry point for repository operations
- Protects invariants of contained entities
- Publishes domain events via `AddDomainEvent()`
- Provides meaningful domain methods, not just setters

### Design Principles

1. Keep aggregates small - include only what must be consistent
2. Reference other aggregates by ID, not object reference
3. One transaction should modify one aggregate

## Value Object

Value objects are immutable objects defined by their attributes, not identity.

### Implementation

```csharp
using MiCake.DDD.Domain;

public class Money : ValueObject
{
    public decimal Amount { get; }
    public string Currency { get; }
    
    public Money(decimal amount, string currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(currency);
        
        Amount = amount;
        Currency = currency.ToUpperInvariant();
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
        return new Money(Amount + other.Amount, Currency);
    }
}

// Record-based value object (C# 9+)
public record Address(string Street, string City, string PostalCode) : RecordValueObject;
```

### Key Characteristics

- Inherits from `ValueObject` or `RecordValueObject`
- Immutable after creation
- Equality based on property values
- No identity - replaceable as a whole

## Domain Event

Domain events represent something significant that happened in the domain.

### Implementation

```csharp
using MiCake.DDD.Domain;

public record OrderCreatedEvent(long OrderId, long CustomerId) : IDomainEvent;

public record OrderConfirmedEvent(long OrderId) : IDomainEvent;

// Event handler
public class OrderCreatedEventHandler : IDomainEventHandler<OrderCreatedEvent>
{
    private readonly INotificationService _notifications;
    
    public OrderCreatedEventHandler(INotificationService notifications)
    {
        _notifications = notifications;
    }
    
    public async Task HandleAsync(OrderCreatedEvent @event, CancellationToken cancellationToken = default)
    {
        await _notifications.SendOrderConfirmationAsync(@event.OrderId, @event.CustomerId);
    }
}
```

### Key Characteristics

- Implements `IDomainEvent`
- Named in past tense (OrderCreated, not OrderCreate)
- Immutable (use record types)
- Published via `AddDomainEvent()` in aggregate root

## Domain Service

Domain services contain domain logic that doesn't naturally fit within an entity or value object.

### When to Use

- Logic involves multiple aggregates
- Logic requires external resources
- Complex calculations or algorithms

### Implementation

```csharp
public interface IOrderPricingService
{
    Task<Money> CalculateTotalWithDiscountAsync(Order order, CancellationToken cancellationToken = default);
}

public class OrderPricingService : IOrderPricingService
{
    private readonly IDiscountRepository _discountRepository;
    
    public OrderPricingService(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
    }
    
    public async Task<Money> CalculateTotalWithDiscountAsync(Order order, CancellationToken cancellationToken = default)
    {
        var baseTotal = order.GetTotalAmount();
        var discount = await _discountRepository.GetApplicableDiscountAsync(order.CustomerId, cancellationToken);
        
        if (discount != null)
        {
            return new Money(baseTotal * (1 - discount.Percentage), "USD");
        }
        
        return new Money(baseTotal, "USD");
    }
}
```
