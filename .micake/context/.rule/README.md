# Context Output Rules

Rules for AI agents when generating files in `.micake/context/` directory.

## Audience

Target audience: AI agents (not humans). Optimize for machine parsing.

## Core Principles

1. **Minimal**: Include only essential information
2. **Structured**: Use consistent YAML format
3. **Flat**: Avoid deep nesting (max 3 levels)
4. **Flow Example**: Flow the example list in the yml file, using other formats is not allowed
5. **No prose**: No explanatory paragraphs or descriptions
6. **No visuals**: No diagrams, icons, emojis, or ASCII art

## Format Requirements

### File Format

- Use YAML exclusively
- UTF-8 encoding
- No BOM
- Unix line endings (LF)

### Header Block

Required fields at file start:

```yaml
version: "1.0"
last_updated: "YYYY-MM-DDTHH:MM:SSZ"
```

### Naming Conventions

- Files: lowercase with hyphens (e.g., `project-structure.yaml`)
- Keys: snake_case
- Values: exact names from source code (preserve casing)

## Content Guidelines

### DO

- Use arrays for lists of items
- Use short, single-line values
- Include file paths relative to project root
- Use enums for categorical values
- Keep descriptions under 80 characters if needed

### DO NOT

- Include comments explaining "why"
- Add examples or sample data
- Use multi-line strings
- Include timestamps other than `last_updated`
- Add metadata not required by consumers

## File-Specific Rules

### project-structure.yaml

Required fields per project:

```yaml
projects:
  - name: "ProjectName"           # Required: exact project name
    path: "relative/path"         # Required: path from solution root
    type: "domain|application|infrastructure|web|test"
    framework: "net8.0"           # Target framework
    dependencies: []              # Project references
    key_classes: []               # Important classes only
```

Skip optional fields if empty. Do not include:
- Build configurations
- NuGet package lists
- Generated files
- Test fixtures

### domain-model.yaml

Required fields per aggregate:

```yaml
bounded_contexts:
  - name: "ContextName"
    aggregates:
      - name: "AggregateName"
        root: "RootEntityName"
        entities: []              # Child entity names only
        value_objects: []         # Value object names only
        domain_events: []         # Event names only
```

Do not include:
- Property details
- Method signatures
- Implementation details
- Invariant descriptions

### module-dependencies.yaml

```yaml
dependencies:
  - source: "ModuleA"
    target: "ModuleB"
    type: "reference|event|api"
```

Only include direct dependencies. Do not include transitive dependencies.

## Size Limits

- Single file: max 500 lines
- Array items: max 50 per array
- If exceeded: split into multiple files by bounded context

## Validation

Before saving, verify:

1. YAML syntax is valid
2. All required fields present
3. No duplicate entries
4. Paths exist in project
5. Names match source code exactly
