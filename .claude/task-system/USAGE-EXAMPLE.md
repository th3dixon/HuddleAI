# Generic Task System Usage Example

## Complete Workflow Example

This example shows how to use the generic task system for any project.

### Step 1: Initialize the System

```bash
# For a React web application
claude-code run .claude/commands/init-generic-tasks.md \
  --project-name="MyTodoApp" \
  --project-type=web \
  --language=typescript \
  --framework=react
```

This creates:
```
.claude/
├── task-system/
│   ├── config.json
│   ├── tasks/
│   │   ├── master-list.json
│   │   ├── active-queue.json
│   │   └── status.json
│   └── templates/
└── commands/
    ├── next-generic-task.md
    ├── complete-task.md
    └── ...
```

### Step 2: Generate Tasks from Specification

Create a simple spec file `todo-app-spec.md`:
```markdown
# Todo App Specification

## Features
1. User can add new todos
2. User can mark todos as complete
3. User can delete todos
4. User can filter todos (all, active, completed)
5. Data persists in localStorage

## Technical Requirements
- React with TypeScript
- Styled Components for styling
- Jest for testing
- No external state management (use React hooks)
```

Generate tasks:
```bash
claude-code run .claude/generators/from-spec.md todo-app-spec.md
```

This generates tasks like:
```json
[
  {
    "id": "TASK-001",
    "title": "Initialize React TypeScript project",
    "type": "config",
    "priority": 1,
    "files": {
      "create": ["package.json", "tsconfig.json", "src/index.tsx"]
    }
  },
  {
    "id": "TASK-002",
    "title": "Create Todo type definition",
    "type": "feature",
    "priority": 2,
    "files": {
      "create": ["src/types/Todo.ts"]
    },
    "dependencies": {
      "requires": ["TASK-001"]
    }
  },
  {
    "id": "TASK-003",
    "title": "Implement TodoItem component",
    "type": "feature",
    "priority": 3,
    "files": {
      "create": ["src/components/TodoItem.tsx", "src/components/TodoItem.test.tsx"]
    },
    "dependencies": {
      "requires": ["TASK-002"]
    }
  }
  // ... more tasks
]
```

### Step 3: Check Initial Status

```bash
claude-code run .claude/commands/task-status.md
```

Output:
```
Task Queue Status
=================
Project: MyTodoApp
Type: web (React/TypeScript)

Total Tasks:      15
Completed:        0 (0%)
Active:          0
Queued:          3
Pending:         12
Blocked:         0

Next Available:
1. TASK-001: Initialize React TypeScript project (30 min)
2. TASK-004: Setup styled-components (15 min)
3. TASK-005: Create base App structure (20 min)
```

### Step 4: Get and Execute First Task

```bash
# Get next task
claude-code run .claude/commands/next-generic-task.md
```

Output:
```json
{
  "task": {
    "id": "TASK-001",
    "title": "Initialize React TypeScript project",
    "instructions": {
      "files": {
        "create": ["package.json", "tsconfig.json", "src/index.tsx"]
      },
      "commands": [
        "npx create-react-app . --template typescript",
        "npm install styled-components @types/styled-components"
      ],
      "validation": [
        "Project runs with npm start",
        "TypeScript compilation succeeds",
        "No errors in console"
      ]
    }
  }
}
```

### Step 5: Complete the Task

After executing:
```bash
claude-code run .claude/commands/complete-task.md TASK-001 --status=completed
```

Output:
```json
{
  "success": true,
  "task": "TASK-001",
  "newlyAvailable": [
    "TASK-002: Create Todo type definition",
    "TASK-003: Implement TodoItem component"
  ],
  "progress": {
    "completed": 1,
    "total": 15,
    "percentage": 6.7
  }
}
```

### Step 6: Work with Multiple Agents

Get tasks for parallel execution:
```bash
claude-code run .claude/commands/parallel-tasks.md --max=3
```

Output:
```json
{
  "tasks": [
    {
      "id": "TASK-002",
      "title": "Create Todo type definition",
      "agent": "agent-1"
    },
    {
      "id": "TASK-004",
      "title": "Setup styled-components",
      "agent": "agent-2"
    },
    {
      "id": "TASK-005",
      "title": "Create base App structure",
      "agent": "agent-3"
    }
  ],
  "parallelTime": "20 minutes",
  "sequentialTime": "50 minutes",
  "efficiency": "60% time saved"
}
```

### Step 7: Add Custom Task

```bash
claude-code run .claude/commands/add-task.md \
  --title="Add dark mode support" \
  --type=feature \
  --priority=3 \
  --files-create="src/hooks/useDarkMode.ts" \
  --files-modify="src/App.tsx" \
  --depends-on="TASK-005"
```

### Step 8: Monitor Progress

```bash
# Watch progress in real-time
claude-code run .claude/commands/task-status.md --watch
```

## Different Project Types

### Python API Example

Initialize:
```bash
init-generic-tasks.md \
  --project-name="UserAPI" \
  --project-type=api \
  --language=python \
  --framework=fastapi
```

Generated tasks would include:
- Setup FastAPI project structure
- Create database models
- Implement CRUD endpoints
- Add authentication
- Create API documentation
- Write tests

### Mobile App Example

Initialize:
```bash
init-generic-tasks.md \
  --project-name="WeatherApp" \
  --project-type=mobile \
  --language=kotlin \
  --framework=android
```

Generated tasks would include:
- Create Android project
- Setup navigation
- Implement weather API client
- Create UI screens
- Add location services
- Implement offline support

## Key Benefits

1. **Works with ANY project** - Not tied to specific technology
2. **Consistent interface** - Same commands for all projects
3. **Flexible task structure** - Adapts to project needs
4. **Parallel execution** - Built for multi-agent work
5. **Clear progress tracking** - Always know where you are

## Advanced Usage

### Custom Task Types
Edit `.claude/task-system/config.json`:
```json
{
  "workflow": {
    "types": [
      "feature", "bugfix", "refactor", 
      "test", "docs", "design", "review"
    ]
  }
}
```

### Project-Specific Validation
Add to task:
```json
{
  "validation": {
    "criteria": ["All tests pass"],
    "tests": {
      "command": "npm test",
      "expected": "exit code 0"
    },
    "custom": {
      "lintCommand": "npm run lint",
      "buildCommand": "npm run build"
    }
  }
}
```

### Integration with CI/CD
```json
{
  "integration": {
    "ci": {
      "enabled": true,
      "provider": "github-actions",
      "onComplete": "create-pr",
      "runTests": true
    }
  }
}
```

This generic system provides a consistent, reliable way to manage tasks for any project type while remaining flexible enough to handle project-specific needs.