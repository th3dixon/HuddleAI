# Task Queue Management System Design

## Overview
A robust, repeatable task management system that maintains consistent structure and workflow across all executions.

## Core Principles
1. **Single Source of Truth**: One master task list with clear status tracking
2. **Dependency Management**: Automatic task ordering based on dependencies
3. **Parallel Execution**: Identify and queue independent tasks for concurrent work
4. **Status Tracking**: Clear, consistent status progression
5. **Idempotent Operations**: Same input always produces same output

## File Structure

```
.claude/
├── task-system/
│   ├── master-tasks.json          # Complete task list with all metadata
│   ├── task-queue.json            # Current execution queue
│   ├── task-status.json           # Real-time status tracking
│   ├── completed-tasks.json       # Archive of completed work
│   └── task-dependencies.json     # Dependency graph
├── commands/
│   ├── next-task.md               # Get next available task(s)
│   ├── complete-task.md           # Mark task as complete
│   ├── queue-status.md            # View queue status
│   └── parallel-tasks.md          # Get all available parallel tasks
└── templates/
    ├── task-template.json         # Standard task structure
    └── status-template.json       # Status tracking template
```

## Task Structure

```json
{
  "taskId": "TASK-001",
  "name": "Initialize ASP.NET Core Solution",
  "description": "Create base solution structure with projects",
  "phase": "1-foundation",
  "priority": 1,
  "estimatedHours": 0.5,
  "dependencies": [],
  "blockedBy": [],
  "blocks": ["TASK-002", "TASK-003"],
  "status": "QUEUED",
  "assignedTo": null,
  "startedAt": null,
  "completedAt": null,
  "verificationCriteria": [
    "Solution builds successfully",
    "All project references correct",
    "Target framework is .NET 9.0"
  ],
  "files": {
    "create": [
      "/src/IdentityServer.sln",
      "/src/IdentityServer/IdentityServer.csproj"
    ],
    "modify": [],
    "delete": []
  },
  "commands": [
    "dotnet new sln -n IdentityServer",
    "dotnet new web -n IdentityServer"
  ],
  "outputs": {
    "artifacts": [],
    "logs": [],
    "errors": []
  }
}
```

## Task Status Lifecycle

```
PENDING → QUEUED → IN_PROGRESS → VERIFICATION → COMPLETED
                ↓                ↓
             BLOCKED          FAILED → RETRY → QUEUED
```

### Status Definitions
- **PENDING**: Task exists but dependencies not met
- **QUEUED**: Ready for execution (all dependencies met)
- **IN_PROGRESS**: Currently being worked on
- **VERIFICATION**: Work done, running verification
- **COMPLETED**: Successfully completed and verified
- **BLOCKED**: Cannot proceed due to external factors
- **FAILED**: Execution failed, needs investigation
- **RETRY**: Failed task ready for retry

## Queue Management Algorithm

```python
def get_next_tasks(max_parallel=3):
    """Get next available tasks respecting dependencies"""
    available_tasks = []
    in_progress_count = count_tasks_by_status("IN_PROGRESS")
    
    if in_progress_count >= max_parallel:
        return []
    
    for task in all_tasks:
        if task.status != "QUEUED":
            continue
            
        # Check all dependencies completed
        deps_met = all(
            get_task(dep_id).status == "COMPLETED" 
            for dep_id in task.dependencies
        )
        
        if deps_met:
            available_tasks.append(task)
            
    # Sort by priority and phase
    available_tasks.sort(key=lambda t: (t.phase, t.priority))
    
    # Return up to max_parallel tasks
    return available_tasks[:max_parallel - in_progress_count]
```

## Execution Workflow

### 1. Initialize Project
```bash
# Run once at project start
claude-code run .claude/commands/init-task-system.md <spec-file>
```

### 2. Get Next Task(s)
```bash
# Single task
claude-code run .claude/commands/next-task.md

# Multiple parallel tasks
claude-code run .claude/commands/parallel-tasks.md --max=3
```

### 3. Execute Task
Each task execution follows this pattern:
1. Read task details from queue
2. Update status to IN_PROGRESS
3. Execute task operations
4. Run verification criteria
5. Update status to COMPLETED or FAILED
6. Update dependency graph
7. Queue newly available tasks

### 4. Complete Task
```bash
claude-code run .claude/commands/complete-task.md TASK-001
```

## Task Categories

### Phase 1: Foundation (TASK-001 to TASK-020)
- Solution setup
- Database infrastructure
- Core models
- Base services

### Phase 2: Identity Core (TASK-021 to TASK-040)
- User management
- Authentication
- Authorization
- MFA

### Phase 3: OAuth/OIDC (TASK-041 to TASK-060)
- OAuth flows
- Token management
- Client registration
- Scopes

### Phase 4: SAML (TASK-061 to TASK-070)
- SAML protocols
- Certificate management
- Metadata

### Phase 5: Management (TASK-071 to TASK-085)
- Admin UI
- Self-service
- APIs

### Phase 6: Security (TASK-086 to TASK-095)
- Advanced auth
- Monitoring
- Compliance

### Phase 7: Testing (TASK-096 to TASK-100)
- Unit tests
- Integration tests
- Performance tests

## Dependency Management

Dependencies are tracked in multiple ways:
1. **Direct Dependencies**: Task A must complete before Task B
2. **Resource Dependencies**: Tasks that modify same files
3. **Logical Dependencies**: Business logic requirements
4. **Phase Dependencies**: All Phase 1 before Phase 2

## Parallel Execution Rules

Tasks can run in parallel if:
1. No shared dependencies
2. No file conflicts
3. Different feature areas
4. Same phase
5. Combined resource usage under limits

## Status Reporting

### Queue Status Command Output
```
Queue Status Report
==================
Total Tasks: 100
Completed: 15 (15%)
In Progress: 3
Queued: 5
Pending: 77
Failed: 0

Current Phase: 1-Foundation (75% complete)

In Progress:
- TASK-016: Create User Repository (Agent-1) [2h]
- TASK-017: Setup Email Service (Agent-2) [1h]
- TASK-018: Create Tenant Service (Agent-3) [0.5h]

Next in Queue:
- TASK-019: Implement Caching Layer
- TASK-020: Create Security Headers

Blocked Tasks: None
Failed Tasks: None

Estimated Completion: 2 days (at current velocity)
```

## Error Handling

### Retry Strategy
1. Automatic retry for transient failures
2. Manual intervention for logical errors
3. Dependency re-evaluation on failure
4. Rollback capabilities for file operations

### Failure Categories
- **Transient**: Network, temporary file locks
- **Configuration**: Missing dependencies, wrong versions
- **Logic**: Code errors, test failures
- **Resource**: Disk space, memory

## Integration with Sub-Agents

### Agent Communication Protocol
```json
{
  "action": "EXECUTE_TASK",
  "taskId": "TASK-016",
  "agent": "agent-1",
  "timeout": 3600,
  "context": {
    "workingDirectory": "/home/projects/idstest1",
    "previousTasks": ["TASK-001", "TASK-002"],
    "sharedResources": {}
  }
}
```

### Agent Response Format
```json
{
  "taskId": "TASK-016",
  "status": "COMPLETED",
  "duration": 1800,
  "filesCreated": ["/src/Repositories/UserRepository.cs"],
  "filesModified": [],
  "verificationResults": {
    "passed": 3,
    "failed": 0,
    "details": []
  },
  "logs": [],
  "errors": []
}
```

## Benefits

1. **Predictable**: Same tasks in same order every time
2. **Scalable**: Easy to add sub-agents for parallel work
3. **Trackable**: Clear visibility into progress
4. **Recoverable**: Can resume from any point
5. **Auditable**: Complete history of all operations

## Implementation Notes

1. Use JSON for data storage (human-readable, easy to edit)
2. Implement file locking for concurrent access
3. Regular backups of task state
4. Validation before state changes
5. Comprehensive logging for debugging