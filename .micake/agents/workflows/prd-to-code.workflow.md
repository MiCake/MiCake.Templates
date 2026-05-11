# PRD to Code Workflow

Complete workflow from PRD/requirements to working MiCake code.

## Overview

This workflow guides the transformation from Product Requirements Document (PRD) or User Stories through domain analysis, architecture design, and code generation to produce complete MiCake-compliant code.

## Phases

### Phase 1: Requirements Analysis (Sage)

**Document discovery:**

Scan `.micake/requirements/` directory for:
- `*.md` - Markdown PRD documents
- `*.txt` - Plain text requirements
- `*.yaml` - Structured requirements

**Extraction patterns:**

```yaml
user_story: "As a {role} I want {feature} so that {benefit}"
acceptance_criteria: "Given {context} When {action} Then {result}"
business_rule: "Rule: {description}"
```

**Output: Domain Concepts List**

```yaml
concepts:
  - name: "Concept name"
    type: "aggregate | entity | value_object | service"
    description: "What this concept represents"
    related_stories: ["US-001", "US-002"]
    
business_rules:
  - id: "BR-001"
    description: "Rule description"
    applies_to: "Concept name"
```

### Phase 2: Domain Modeling (Architect)

**Design activities:**

1. Define aggregate boundaries
2. Design entities and value objects
3. Plan domain events
4. Design module structure

**Output: Domain Model Design**

```yaml
aggregates:
  - name: "AggregateName"
    root_entity: "RootEntity"
    child_entities: ["Entity1", "Entity2"]
    value_objects: ["VO1", "VO2"]
    invariants:
      - "Invariant description"
    events:
      - name: "EventName"
        trigger: "When action occurs"

modules:
  - name: "ModuleName"
    aggregates: ["Aggregate1", "Aggregate2"]
    dependencies: ["OtherModule"]
```

**Design validation:**

- Aggregate boundaries follow transactional consistency requirements
- Value objects are immutable and have no identity
- Domain events capture state changes
- Module dependencies form a DAG (no cycles)

### Phase 3: Code Generation (Developer)

**Generate for each aggregate:**

1. Aggregate root class
2. Child entity classes
3. Value object classes
4. Repository interface
5. Repository implementation
6. Domain events (if applicable)
7. Event handlers (if applicable)
8. EF Core entity configurations

**Code standards:**

- Private setters for domain properties
- Static factory methods for entity creation
- Guard clauses for invariant enforcement
- XML documentation for public members

### Phase 4: Validation (Inspector)

**Code review checklist:**

- [ ] DDD patterns correctly applied
- [ ] Aggregate boundaries respected
- [ ] No anemic domain models
- [ ] Business logic in domain layer
- [ ] Infrastructure concerns separated
- [ ] Proper encapsulation maintained
- [ ] Code performance and bad smells checked
- [ ] Code maintains readability and maintainability

**Architecture compliance:**

- [ ] Layer dependencies correct
- [ ] No circular references
- [ ] Repository pattern followed
- [ ] Domain events for cross-aggregate communication (If applicable)

**Output: Review Report**

```yaml
status: "pass | fail | warning"
issues:
  - severity: "error | warning | info"
    location: "File path and line"
    description: "Issue description"
    suggestion: "How to fix"
recommendations:
  - "Recommendation 1"
  - "Recommendation 2"
```

## Workflow Execution

### Prerequisites

1. Load configuration from `.micake/agents/config/preferences.yaml`
2. Verify project structure exists
3. Check for existing aggregates to avoid conflicts

### Execution Order

```
Sage: Analyze requirements
  ↓
Architect: Design domain model
  ↓
User: Review and approve design
  ↓
Developer: Generate code
  ↓
Inspector: Validate output
  ↓
QA: Verify requirements compliance
  ↓
User: Final review
```

### Error Handling

- If requirements are unclear: Sage requests clarification
- If design conflicts exist: Architect proposes resolution
- If code generation fails: Developer reports specific errors
- If validation fails: Inspector provides fix suggestions
- If requirements not met: QA reports discrepancies

## Output Summary

- Complete domain layer code
- Infrastructure implementations
- Module configuration
- Review report with any issues

## Related Workflows

- `new-project.workflow.md` - For initial project setup
- `create-aggregate.workflow.md` - For single aggregate creation
