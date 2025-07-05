# Initialize Generic Task System

## Purpose
Initialize a task management system for any type of project with customizable configuration.

## Usage
```bash
claude-code run .claude/commands/init-generic-tasks.md [options]

Options:
  --project-name    Name of the project
  --project-type    Type: web|api|library|mobile|cli|service|other
  --language        Primary language (optional)
  --framework       Framework being used (optional)
  --spec-file       Path to specification file (optional)
  --interactive     Interactive setup mode (default)
```

## Process

### 1. Interactive Setup (Default)

If no options provided, enter interactive mode:

```
Task System Initialization
=========================

Project name: [Enter project name]
Project type: 
  1. Web Application
  2. API/Backend Service
  3. Library/Package
  4. Mobile Application
  5. CLI Tool
  6. Desktop Application
  7. Other/Custom

Select type [1-7]: 

Primary language: [e.g., python, javascript, java, etc.]
Framework (optional): [e.g., react, django, spring, etc.]

Task ID prefix [TASK]: 
Maximum parallel tasks [3]: 
Enable task phases? [y/N]: 
```

### 2. Create Directory Structure

```bash
mkdir -p .claude/task-system/tasks
mkdir -p .claude/task-system/tasks/completed
mkdir -p .claude/task-system/state
mkdir -p .claude/task-system/templates
mkdir -p .claude/commands
mkdir -p .claude/generators
mkdir -p .claude/plugins
```

### 3. Generate Configuration

Create `.claude/task-system/config.json`:
```json
{
  "project": {
    "name": "${PROJECT_NAME}",
    "type": "${PROJECT_TYPE}",
    "created": "${TIMESTAMP}",
    "version": "0.1.0"
  },
  "environment": {
    "language": "${LANGUAGE}",
    "framework": "${FRAMEWORK}",
    "tools": []
  },
  "taskSettings": {
    "idPrefix": "${ID_PREFIX}",
    "maxParallel": ${MAX_PARALLEL},
    "autoQueue": true,
    "defaultPriority": 3
  },
  "workflow": {
    "statuses": [
      "pending", "queued", "active", 
      "review", "completed", "blocked", "failed"
    ],
    "types": [
      "feature", "bugfix", "refactor", 
      "test", "docs", "chore", "config"
    ]
  }
}
```

### 4. Initialize Task Files

#### master-list.json
```json
{
  "version": "1.0",
  "created": "${TIMESTAMP}",
  "lastModified": "${TIMESTAMP}",
  "tasks": []
}
```

#### active-queue.json
```json
{
  "queue": [],
  "active": [],
  "lastUpdated": "${TIMESTAMP}"
}
```

#### status.json
```json
{
  "summary": {
    "total": 0,
    "completed": 0,
    "active": 0,
    "queued": 0,
    "pending": 0,
    "blocked": 0,
    "failed": 0
  },
  "lastUpdated": "${TIMESTAMP}"
}
```

### 5. Create Task Schema

`.claude/task-system/templates/task.schema.json`:
```json
{
  "$schema": "http://json-schema.org/draft-07/schema#",
  "type": "object",
  "required": ["id", "title", "type"],
  "properties": {
    "id": {
      "type": "string",
      "pattern": "^${ID_PREFIX}-[0-9]+$"
    },
    "title": {
      "type": "string",
      "minLength": 3,
      "maxLength": 100
    },
    "type": {
      "type": "string",
      "enum": ["feature", "bugfix", "refactor", "test", "docs", "chore", "config"]
    }
  }
}
```

### 6. Install Core Commands

Copy generic commands:
- `next-task.md`
- `complete-task.md`
- `task-status.md`
- `add-task.md`
- `parallel-tasks.md`

### 7. Setup Generators

Based on project type, install relevant generators:

#### Web Projects
- `from-components.md` - Generate tasks from component list
- `from-routes.md` - Generate tasks from route definitions

#### API Projects
- `from-endpoints.md` - Generate tasks from API spec
- `from-models.md` - Generate tasks from data models

#### General
- `from-spec.md` - Generate from specification file
- `from-todo.md` - Extract from TODO comments
- `from-outline.md` - Generate from outline

### 8. Create Initial Tasks (Optional)

If spec file provided:
```bash
claude-code run .claude/generators/from-spec.md ${SPEC_FILE}
```

Otherwise, create setup tasks:
```json
{
  "id": "${PREFIX}-001",
  "title": "Complete project setup",
  "type": "config",
  "description": "Finish configuring the project environment"
}
```

## Output

After initialization:
```
Task System Initialized Successfully!
====================================

Project: ${PROJECT_NAME}
Type: ${PROJECT_TYPE}
Task Prefix: ${ID_PREFIX}

Files created:
✓ Configuration file
✓ Task storage
✓ Command scripts
✓ Generators

Next steps:
1. Add tasks using: add-task.md
2. Or import from spec: from-spec.md <file>
3. View status: task-status.md
4. Get started: next-task.md

Configuration file: .claude/task-system/config.json
```

## Examples

### React Web App
```bash
init-generic-tasks.md \
  --project-name="MyApp" \
  --project-type=web \
  --language=typescript \
  --framework=react
```

### Python API
```bash
init-generic-tasks.md \
  --project-name="UserService" \
  --project-type=api \
  --language=python \
  --framework=fastapi
```

### Generic Project
```bash
init-generic-tasks.md --interactive
```

## Customization

After initialization, customize by:
1. Editing `config.json`
2. Adding custom task types
3. Creating project-specific generators
4. Adding validation rules
5. Installing plugins