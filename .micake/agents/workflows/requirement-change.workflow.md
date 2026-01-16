# Requirement Change Workflow

End-to-end workflow for handling requirement changes and code adjustments.

## Overview

This workflow manages requirement changes from intake through implementation, ensuring proper analysis, planning, code adjustment, and documentation.

## Phases

### Phase 1: Receive Change (Conductor)

**Check conflicts:**

```yaml
check_conflicts:
  scan: ".micake/changes/in-progress/"
  if_exists: "Notify user, ask to wait or force proceed"
  if_empty: "Create change lock, continue"
```

**Receive and validate:**

```yaml
receive_change:
  actions:
    - Parse user's change description
    - Generate change ID: "CR-YYYYMMDD-XXX"
    - Create: ".micake/changes/pending/{change_id}/"
    - Save: "change-request.yaml"

check_requirements:
  scan: ".micake/requirements/"
  if_exists:
    - Load existing requirements
    - Extract key points for comparison
  if_not_exists:
    - Ask user to provide requirements
    - Or generate skeleton from change request
    - Save to ".micake/requirements/" after confirmation
```

**Output: change-request.yaml**

```yaml
id: "CR-YYYYMMDD-XXX"
created_at: "ISO8601"
status: "pending"
description: "Brief change description"
details: "Detailed requirements"
requester: "user"
```

### Phase 2: Analyze Requirements (Sage + Architect)

**Diff analysis (Sage):**

```yaml
diff_analysis:
  compare: "Original requirements vs change request"
  identify:
    - type: "ADD | MODIFY | DELETE"
    - affected_concepts: ["concept1", "concept2"]
  if_unclear:
    - Generate clarification questions
    - Wait for user response
    - Re-analyze
```

**Impact assessment (Architect):**

```yaml
impact_assessment:
  analyze:
    - domain_model: "Aggregates, entities, value objects"
    - dependencies: "Module relationships"
    - api: "Interface changes"
    - database: "Schema changes"
    - tests: "Affected test cases"
  evaluate:
    - risk_level: "HIGH | MEDIUM | LOW"
    - effort_estimate: "Hours range"
```

**Output: impact-report.md**

```markdown
# Impact Report: {change_id}

## Summary
Brief description of the change.

## Affected Areas
- Domain: [list]
- API: [list]
- Database: [list]

## Risk Level: {HIGH|MEDIUM|LOW}

## Effort Estimate: {X-Y hours}

## Recommendations
1. Recommendation 1
2. Recommendation 2
```

### Phase 3: Create Plan (Conductor + Architect)

**Task breakdown:**

```yaml
task_breakdown:
  for_each_change:
    - task_id: "T-001"
    - title: "Task title"
    - type: "code_change | test_update | doc_update"
    - files: ["file1.cs", "file2.cs"]
    - effort: "Xh"
    - priority: "critical | high | medium | low"
    - depends_on: []
```

**Create snapshot (if Git available):**

```yaml
create_snapshot:
  if_git:
    - Create branch: "change/{change_id}"
    - Record commit hash
  if_no_git:
    - Record affected file paths for manual backup
```

**Output: adjustment-plan.yaml**

```yaml
change_id: "CR-YYYYMMDD-XXX"
created_at: "ISO8601"
status: "pending_approval"
branch: "change/{change_id}"
tasks:
  - id: "T-001"
    title: "Task title"
    type: "code_change"
    files: ["path/to/file.cs"]
    effort: "2h"
    priority: "high"
    depends_on: []
execution_order: ["T-001", "T-002"]
total_effort: "Xh"
```

### Phase 4: Implement Changes (Developer + Inspector)

**Execute tasks (Developer):**

```yaml
execute_tasks:
  for_each_task:
    - Read task details from plan
    - Locate affected files
    - Implement changes following DDD patterns
    - Update related tests
    - Mark task as "completed"
```

**Code review (Inspector):**

```yaml
code_review:
  check:
    - DDD pattern compliance
    - Code quality standards
    - Proper encapsulation
  if_issues:
    - Return to Developer for fixes
    - Repeat until passed
```

### Phase 5: Validate (QA)

**Requirements validation:**

```yaml
validate:
  load: "Updated requirements"
  verify:
    - Each change point implemented correctly
    - Edge cases handled
    - Error scenarios managed
  output: "validation-report.md"
```

**Regression check:**

```yaml
regression_check:
  analyze: "Changed modules"
  identify: "Potential regression risks"
  suggest: "Test cases to run"
```

### Phase 6: Finalize (Conductor)

**Update documentation:**

```yaml
update_docs:
  - Merge changes into original requirements
  - Add change history entry
  - Update version number
```

**Generate changelog:**

```yaml
changelog:
  output: ".micake/changes/completed/{change_id}/CHANGELOG.md"
  content:
    - Change ID and date
    - Summary
    - Modified files list
```

**Archive and cleanup:**

```yaml
archive:
  - Move from "pending/" or "in-progress/" to "completed/"
  - Update status to "completed"
  - Prompt: "Delete change history? (recommended)"
```

## Rollback Workflow

```yaml
rollback:
  trigger: "User request or Phase 4/5 failure"
  process:
    - if_git: "Switch to original branch, delete change branch"
    - Restore requirements to pre-change version
    - Move record to "failed/" directory
    - Generate rollback report
```

## Related Commands

| Command | Description |
|---------|-------------|
| change-request | Start change workflow |
| change-status | View current change status |
| change-history | View completed changes |
| change-rollback | Rollback a change |
| change-cleanup | Delete change history |
| sync-context | Refresh project context |

## Related Workflows

- `prd-to-code.workflow.md` - Full PRD to code workflow
- `create-aggregate.workflow.md` - Single aggregate creation
