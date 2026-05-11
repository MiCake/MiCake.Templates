# MiCake Agent System

AI-Powered Development Assistant for MiCake Framework.

## Overview

MiCake Agent System is an AI assistant designed for MiCake framework users, helping developers build DDD-compliant .NET applications.

## Features

- **Smart Guidance** - Complete development chain from requirements to code
- **Architecture Design** - DDD domain modeling and module planning
- **Code Generation** - Automatic generation of MiCake-compliant code
- **Code Review** - Automated review based on best practices
- **Knowledge Base** - Built-in MiCake framework knowledge

## Quick Start

1. Ensure `.micake/agents/` folder exists in your project
2. Select **Conductor** agent in GitHub Copilot Chat to get started
3. Run `setup` to configure your preferences
4. Use `services` to see all available agents

## Commands

| Command | Description | Agent |
|---------|-------------|-------|
| `start` | Welcome and introduction | Conductor |
| `setup` | Configure preferences | Conductor |
| `init-project` | Initialize MiCake project | Conductor |
| `services` | List all available agents | Conductor |
| `change-request` | Start requirement change workflow | Conductor |
| `change-status` | View current change status | Conductor |
| `change-history` | View completed changes | Conductor |
| `change-rollback` | Rollback a change | Conductor |
| `change-cleanup` | Delete change history | Conductor |
| `sync-context` | Refresh project context | Conductor |
| `analyze-requirements` | Analyze PRD/User Stories | Sage |
| `design-aggregate` | Design aggregate boundaries | Architect |
| `implement` | Implement feature from PRD/Story | Developer |
| `create-aggregate` | Create aggregate root | Developer |
| `review` | Code review | Inspector |
| `validate` | Validate against requirements | QA |
| `consult` | Get expert advice | Consultant |

## Agents

| Agent | Role |
|-------|------|
| [Conductor](./conductor.agent.md) | Main coordinator, setup guide, agent routing |
| [Sage](./sage.agent.md) | Requirements analysis, domain concept extraction |
| [Architect](./architect.agent.md) | Architecture design, domain modeling |
| [Developer](./developer.agent.md) | Code implementation, bug fixing |
| [Inspector](./inspector.agent.md) | Code review, DDD compliance |
| [QA](./qa.agent.md) | Requirements validation, quality assurance |
| [Consultant](./consultant.agent.md) | Framework guidance, best practices |

## Directory Structure

```
.micake/agents/
├── README.md                        # This file
├── conductor.agent.md               # Conductor Agent (start here)
├── sage.agent.md                    # Sage Agent
├── architect.agent.md               # Architect Agent
├── developer.agent.md               # Developer Agent
├── inspector.agent.md               # Inspector Agent
├── qa.agent.md                      # QA Agent
├── consultant.agent.md              # Consultant Agent
│
├── config/
│   └── preferences.yaml             # User preferences
│
├── knowledge/                       # Knowledge base
│   ├── README.md
│   ├── ddd-patterns.md
│   ├── module-system.md
│   ├── repository-patterns.md
│   └── troubleshooting.md
│
├── workflows/                       # Workflow definitions
│   ├── new-project.workflow.md
│   ├── create-aggregate.workflow.md
│   ├── prd-to-code.workflow.md
│   └── requirement-change.workflow.md
│
└── templates/                       # Code templates
    ├── aggregate.template.cs
    ├── entity.template.cs
    ├── value-object.template.cs
    ├── repository.template.cs
    ├── domain-event.template.cs
    └── module.template.cs

.micake/context/                     # Project context (auto-generated)
├── project-structure.yaml           # Project code structure cache
└── domain-model.yaml                # Domain model summary

.micake/changes/                     # Change management
├── pending/                         # Pending changes
├── in-progress/                     # In-progress changes
├── completed/                       # Completed changes
└── failed/                          # Failed/rolled-back changes
```

## Configuration

Configure preferences in `.micake/agents/config/preferences.yaml`.

## Change Management

Use `change-request` command to handle requirement changes. The workflow includes:
1. Conflict detection and change intake
2. Requirements diff analysis and impact assessment
3. Task planning and prioritization
4. Code implementation and review
5. Validation and documentation sync
6. Optional history cleanup via `change-cleanup`

## Requirements Documents

Import external requirement documents (PRD, User Stories) by placing them in `.micake/requirements/` directory.

## References

- [MiCake Documentation](https://micake.github.io/)
- [MiCake GitHub Repository](https://github.com/MiCake/MiCake)
