# Change Output Rules

Rules for AI agents when generating files in `.micake/changes/` directory.

## Audience

Target audience: AI agents (not humans). Optimize for machine parsing and workflow automation.

## Core Principles

1. **Actionable**: Every item must be executable
2. **Traceable**: Link to source requirements
3. **Atomic**: One concern per file
4. **Stateful**: Always include current status
5. **No visuals**: No diagrams, icons, emojis, or ASCII art

## Format Requirements

### File Formats

- `.yaml` for structured data (change-request, adjustment-plan)
- `.md` for reports (impact-report, changelog)

### Markdown Rules (for .md files)

- Use flat structure: H1 for title, H2 for sections only
- No H3 or deeper headings
- Use bullet lists, not numbered lists
- No tables (use bullet lists instead)
- No code blocks unless showing exact file content
- Keep sections under 10 lines each

### YAML Rules

- Max nesting: 3 levels
- Use arrays for ordered items
- Use enums for status values
- No inline comments

## Status Values

Use exact values:

```yaml
# Change status
status: "pending|in_progress|completed|failed|cancelled"

# Task status
task_status: "not_started|in_progress|completed|blocked"

# Priority
priority: "critical|high|medium|low"

# Risk level
risk_level: "HIGH|MEDIUM|LOW"
```

## File-Specific Rules

### change-request.yaml

```yaml
id: "CR-YYYYMMDD-XXX"
created_at: "ISO8601"
status: "pending"
summary: "One-line description max 100 chars"
change_type: "ADD|MODIFY|DELETE"
affected_areas:
  - "area1"
  - "area2"
requester: "user"
```

Do not include:
- Full requirement text (reference file path instead)
- User conversation history
- Alternative solutions considered

### impact-report.md

```markdown
# Impact Report: CR-YYYYMMDD-XXX

## Summary
One paragraph, max 3 sentences.

## Affected Files
- path/to/file1.cs
- path/to/file2.cs

## Risk Level
HIGH|MEDIUM|LOW

## Effort
X-Y hours

## Dependencies
- Dependency 1
- Dependency 2
```

Do not include:
- Detailed code analysis
- Historical context
- Stakeholder information
- Alternative approaches

### adjustment-plan.yaml

```yaml
change_id: "CR-YYYYMMDD-XXX"
created_at: "ISO8601"
status: "pending_approval"
git_branch: "change/CR-YYYYMMDD-XXX"
total_effort: "Xh"
execution_order: ["T-001", "T-002"]
tasks:
  - id: "T-001"
    title: "Short title max 50 chars"
    type: "code_change|test_update|doc_update|config_change"
    status: "not_started"
    files:
      - "path/to/file.cs"
    effort: "2h"
    priority: "high"
    depends_on: []
```

Rules:
- Task IDs: sequential T-XXX format
- Max 20 tasks per plan
- If more needed: split into phases
- File paths: relative to project root

### CHANGELOG.md

```markdown
# Changelog: CR-YYYYMMDD-XXX

## Date
YYYY-MM-DD

## Summary
One sentence.

## Changes
- Added: feature description
- Modified: what was changed
- Removed: what was removed

## Files Modified
- path/to/file1.cs
- path/to/file2.cs
```

Keep total under 30 lines.

## Directory Structure

```
changes/
├── pending/
│   └── CR-YYYYMMDD-XXX/
│       └── change-request.yaml
├── in-progress/
│   └── CR-YYYYMMDD-XXX/
│       ├── change-request.yaml
│       ├── impact-report.md
│       └── adjustment-plan.yaml
├── completed/
│   └── CR-YYYYMMDD-XXX/
│       ├── change-request.yaml
│       ├── impact-report.md
│       ├── adjustment-plan.yaml
│       └── CHANGELOG.md
└── failed/
    └── CR-YYYYMMDD-XXX/
        └── (same as completed, plus failure reason)
```

## Lifecycle Rules

1. Create `change-request.yaml` in `pending/` on intake
2. Move to `in-progress/` when analysis starts
3. Add files progressively as phases complete
4. Move to `completed/` or `failed/` when done
5. Never modify files after moving to `completed/`

## Size Limits

- change-request.yaml: max 30 lines
- impact-report.md: max 50 lines
- adjustment-plan.yaml: max 100 lines
- CHANGELOG.md: max 30 lines

## Validation

Before saving, verify:

1. Change ID format is correct
2. All status values are valid enums
3. File paths exist or will be created
4. Task dependencies form valid DAG (no cycles)
5. Effort estimates use consistent units (hours)
