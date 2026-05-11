# Create Aggregate Workflow

Step-by-step workflow to create a new aggregate with all related components.

## Overview

This workflow guides the creation of a new aggregate including aggregate root, child entities, value objects, repository, domain events, and EF Core configuration.

## Workflow Steps

### Step 1: Gather Requirements (Sage + Architect)

**Collect aggregate information:**

```yaml
aggregate:
  name: "Aggregate name (e.g., Order, Product)"
  purpose: "Business concept this aggregate represents"
  
identity:
  id_type: "long | Guid | int | string"
  id_generation: "database | application | manual"
  
properties:
  - name: "Property name"
    type: "Type"
    required: true/false
    description: "Description"

child_entities:
  - name: "Entity name"
    relationship: "one-to-many | one-to-one"
    properties:
      - name: "Property name"
        type: "Type"

value_objects:
  - name: "Value object name"
    components:
      - name: "Component name"
        type: "Type"

business_rules:
  - "Rule 1: Description"
  - "Rule 2: Description"

domain_events:
  - trigger: "When..."
    event: "...Event"
```

### Step 2: Design Review (Architect)

**Validate design decisions:**

1. Confirm aggregate boundaries are correct
2. Verify entities vs value objects classification
3. Check business rule placement
4. Review event triggers
5. Get user confirmation before proceeding

**Key questions:**

- Is this aggregate too large? Consider splitting.
- Are referenced entities truly owned by this aggregate?
- Should any properties be value objects instead?

### Step 3: Generate Code (Developer)

**Generate the following files:**

1. **Aggregate Root** (`{Name}.cs`)
   - Entity properties with encapsulation
   - Business methods enforcing invariants
   - Static factory method for creation
   - Domain event raising

2. **Child Entities** (`{ChildName}.cs`)
   - Entity base class inheritance
   - Parent reference if needed
   - Entity-specific business logic

3. **Value Objects** (`{ValueObjectName}.cs`)
   - Immutable properties
   - Equality comparison
   - Validation in constructor

4. **Repository Interface** (`I{Name}Repository.cs`)
   - Standard CRUD operations
   - Custom query methods

5. **Repository Implementation** (`{Name}Repository.cs`)
   - EF Core implementation
   - Query optimization

6. **Domain Events** (`{Name}Events.cs`)
   - Event classes with relevant data
   - Timestamp and correlation ID

7. **EF Core Configuration** (`{Name}Configuration.cs`)
   - Entity mapping
   - Value object configuration
   - Relationship mapping

### Step 4: Register & Validate (Inspector)

**Complete integration:**

1. Add DbSet to DbContext
2. Register repository in DI container
3. Run `dotnet build` to verify compilation
4. Generate migration prompt if needed

**Validation checklist:**

- [ ] Aggregate root inherits from correct base class
- [ ] All properties have proper encapsulation
- [ ] Business rules are enforced in domain methods
- [ ] Repository interface is in Domain layer
- [ ] Repository implementation is in Infrastructure layer
- [ ] EF configuration handles all value objects

## Output Files

```
src/{Project}.Domain/
├── Aggregates/{Name}/
│   ├── {Name}.cs              # Aggregate root
│   ├── {ChildEntity}.cs       # Child entities
│   └── {ValueObject}.cs       # Value objects
├── Events/
│   └── {Name}Events.cs        # Domain events
└── Repositories/
    └── I{Name}Repository.cs   # Repository interface

src/{Project}.Infrastructure/
├── Repositories/
│   └── {Name}Repository.cs    # Repository implementation
└── EntityConfigurations/
    └── {Name}Configuration.cs # EF Core mapping
```

## Related Templates

- `aggregate.template.cs`
- `entity.template.cs`
- `value-object.template.cs`
- `repository-interface.template.cs`
- `repository-impl.template.cs`
- `ef-configuration.template.cs`
