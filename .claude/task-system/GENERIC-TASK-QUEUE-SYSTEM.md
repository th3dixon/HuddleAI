# Generic Task Queue Management System

## Overview
A universal, project-agnostic task management system that can be initialized for any type of project and maintain consistent operation across all implementations.

## Core Design Principles

1. **Project Agnostic**: No hardcoded project-specific logic
2. **Technology Neutral**: Works with any programming language or framework
3. **Flexible Structure**: Adapts to different project organizations
4. **Consistent Interface**: Same commands work for all projects
5. **Extensible**: Easy to add project-specific customizations

## System Architecture

```
.claude/
├── task-system/
│   ├── config.json              # Project-specific configuration
│   ├── tasks/
│   │   ├── master-list.json     # All tasks for the project
│   │   ├── active-queue.json    # Currently queued tasks
│   │   ├── status.json          # Real-time status tracking
│   │   └── completed/           # Archive of completed tasks
│   ├── templates/
│   │   ├── task.schema.json     # Task structure schema
│   │   └── project.schema.json  # Project config schema
│   └── state/
│       ├── dependencies.json    # Task dependency graph
│       └── metrics.json         # Performance metrics
├── commands/
│   ├── init-tasks.md            # Initialize for any project
│   ├── next-task.md             # Get next available task
│   ├── complete-task.md         # Mark task complete
│   ├── task-status.md           # View current status
│   ├── parallel-tasks.md        # Get parallel tasks
│   └── add-task.md              # Add new task to queue
└── generators/
    ├── from-spec.md             # Generate tasks from specification
    ├── from-todo.md             # Generate tasks from TODO comments
    └── from-issues.md           # Generate tasks from issue tracker
```

## Generic Task Structure

```json
{
  "id": "TASK-001",
  "title": "Short descriptive title",
  "description": "Detailed description of what needs to be done",
  "type": "feature|bugfix|refactor|test|docs|config",
  "category": "Project-specific category",
  "priority": 1,
  "effort": {
    "estimated": 30,
    "unit": "minutes",
    "complexity": "low|medium|high"
  },
  "dependencies": {
    "requires": ["TASK-ID"],
    "blocks": ["TASK-ID"]
  },
  "resources": {
    "files": {
      "create": [],
      "modify": [],
      "delete": [],
      "read": []
    },
    "external": {
      "apis": [],
      "services": [],
      "tools": []
    }
  },
  "validation": {
    "criteria": [
      "Generic success criterion"
    ],
    "tests": {
      "command": "Optional test command",
      "expected": "Expected outcome"
    }
  },
  "metadata": {
    "created": "2024-01-13T10:00:00Z",
    "modified": "2024-01-13T10:00:00Z",
    "tags": ["optional", "tags"],
    "custom": {}
  },
  "status": {
    "current": "pending|queued|active|completed|failed|blocked",
    "history": [],
    "assignee": null,
    "progress": 0
  }
}
```

## Project Configuration

```json
{
  "project": {
    "name": "Project Name",
    "type": "web|api|library|mobile|desktop|cli|service",
    "description": "Project description",
    "version": "1.0.0"
  },
  "environment": {
    "language": "python|javascript|java|csharp|go|rust|other",
    "framework": "Optional framework name",
    "platform": "Optional platform",
    "tools": ["List of required tools"]
  },
  "taskSettings": {
    "idPrefix": "TASK",
    "maxParallel": 3,
    "autoQueue": true,
    "priorityLevels": 5
  },
  "workflow": {
    "phases": {
      "enabled": false,
      "list": []
    },
    "categories": [
      "Custom category list"
    ],
    "types": [
      "feature", "bugfix", "refactor", "test", "docs"
    ]
  },
  "validation": {
    "requireTests": false,
    "requireDocs": false,
    "customChecks": []
  },
  "integration": {
    "vcs": {
      "type": "git",
      "autoCommit": false,
      "branchStrategy": "feature-branch"
    },
    "ci": {
      "enabled": false,
      "provider": null
    }
  }
}
```

## Initialization Process

### Step 1: Create Configuration

```bash
claude-code run .claude/commands/init-tasks.md
```

This will:
1. Prompt for project type and details
2. Create the directory structure
3. Generate initial configuration
4. Set up task templates

### Step 2: Generate Tasks

Multiple methods available:

#### From Specification
```bash
claude-code run .claude/generators/from-spec.md <spec-file>
```

#### From Code TODOs
```bash
claude-code run .claude/generators/from-todo.md <source-directory>
```

#### From Issues
```bash
claude-code run .claude/generators/from-issues.md <issue-tracker-url>
```

#### Manual Creation
```bash
claude-code run .claude/commands/add-task.md
```

## Universal Commands

### Get Next Task
```bash
claude-code run .claude/commands/next-task.md
```

Returns task in a consistent format regardless of project:
```json
{
  "task": {
    "id": "TASK-001",
    "title": "Initialize database connection",
    "files": ["src/db/connection.py"],
    "validation": ["Connection test passes"]
  },
  "context": {
    "previousTasks": ["TASK-000"],
    "relatedDocs": ["docs/database.md"]
  }
}
```

### Complete Task
```bash
claude-code run .claude/commands/complete-task.md TASK-001
```

### View Status
```bash
claude-code run .claude/commands/task-status.md
```

## Task Generation Templates

### Feature Template
```json
{
  "type": "feature",
  "pattern": {
    "title": "Implement {feature_name}",
    "subtasks": [
      "Create {feature_name} interface/contract",
      "Implement {feature_name} logic",
      "Add {feature_name} tests",
      "Document {feature_name}"
    ]
  }
}
```

### Bug Fix Template
```json
{
  "type": "bugfix",
  "pattern": {
    "title": "Fix {issue_description}",
    "subtasks": [
      "Reproduce {issue_description}",
      "Implement fix for {issue_description}",
      "Add regression test",
      "Verify fix in multiple scenarios"
    ]
  }
}
```

## Dependency Detection

Automatic dependency detection based on:
1. **File Dependencies**: Tasks modifying same files
2. **Logical Dependencies**: Inferred from task descriptions
3. **Explicit Dependencies**: Manually specified
4. **Pattern Dependencies**: Common patterns (e.g., test depends on implementation)

## Status Management

### Task Lifecycle
```
DRAFT → PENDING → QUEUED → ACTIVE → REVIEW → COMPLETED
           ↓         ↓        ↓        ↓
        BLOCKED   PAUSED   FAILED   REJECTED
```

### Progress Tracking
- Percentage complete (0-100)
- Time tracking (estimated vs actual)
- Blocker tracking
- Notes and comments

## Parallel Execution Rules

Tasks can run in parallel if:
1. No shared file modifications
2. No dependency conflicts
3. Resource limits not exceeded
4. Same priority level (optional)

## Integration Points

### Version Control
```json
{
  "vcs": {
    "preCommit": "Create branch for task",
    "postComplete": "Commit changes",
    "commitMessage": "feat: {task.title} (#{task.id})"
  }
}
```

### External Tools
- Issue trackers (Jira, GitHub Issues)
- CI/CD systems
- Documentation generators
- Test runners

## Extensibility

### Custom Fields
Add project-specific fields to tasks:
```json
{
  "metadata": {
    "custom": {
      "storyPoints": 5,
      "sprint": "2024-Q1-Sprint3",
      "component": "authentication"
    }
  }
}
```

### Plugins
Create project-specific plugins:
```
.claude/plugins/
├── validators/
│   └── code-style.js
├── generators/
│   └── from-swagger.js
└── hooks/
    ├── pre-task.sh
    └── post-task.sh
```

## Example Initializations

### Web Application
```bash
claude-code run init-tasks.md --type=web --framework=react --language=typescript
```

### Python Library
```bash
claude-code run init-tasks.md --type=library --language=python --testing=pytest
```

### Microservice
```bash
claude-code run init-tasks.md --type=service --language=go --platform=kubernetes
```

## Benefits

1. **Truly Generic**: Works with any project type
2. **Consistent Interface**: Same commands everywhere
3. **Flexible Schema**: Adapts to project needs
4. **No Lock-in**: Easy to migrate or modify
5. **Tool Agnostic**: Integrates with any toolchain