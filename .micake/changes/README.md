# Changes Directory

This directory manages requirement change records throughout their lifecycle.

## Output Rules

AI agents must follow rules in `.rule/README.md` when generating files in this directory.

## Structure

```
changes/
├── pending/           # Awaiting analysis or approval
├── in-progress/       # Currently being implemented
├── completed/         # Successfully finished
└── failed/            # Rolled back or cancelled
```

## Change Record Structure

Each change has its own directory named by change ID (e.g., `CR-20260107-001/`):

```
{change_id}/
├── change-request.yaml    # Original change request
├── impact-report.md       # Impact analysis (Phase 2)
├── adjustment-plan.yaml   # Task breakdown (Phase 3)
└── CHANGELOG.md           # Final changelog (Phase 6)
```

## Cleanup

Use `change-cleanup` command to remove old records and keep this directory efficient.
