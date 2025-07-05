# Get Next Task

## Purpose
Retrieve the next available task from the queue, respecting dependencies and current capacity.

## Usage
```
claude-code run .claude/commands/get-next-task.md [--agent=agent-name]
```

## Process

1. **Check Current Capacity**
   - Read task-status.json
   - Count IN_PROGRESS tasks
   - Verify under max parallel limit

2. **Find Available Tasks**
   - Read task-queue.json
   - Filter for QUEUED status
   - Check all dependencies are COMPLETED
   - Verify no file conflicts with IN_PROGRESS tasks

3. **Select Best Task**
   - Sort by phase (earliest first)
   - Sort by priority within phase
   - Consider resource requirements
   - Return highest priority available task

4. **Update Status**
   - Change task status to IN_PROGRESS
   - Record agent assignment
   - Set startedAt timestamp
   - Update queue and status files

5. **Return Task Details**
   - Full task specification
   - List of files to create/modify
   - Verification criteria
   - Any special instructions

## Output Format

```json
{
  "taskId": "TASK-016",
  "name": "Create User Repository",
  "description": "Implement repository pattern for User entity",
  "phase": "1-foundation",
  "files": {
    "create": [
      "/src/Repositories/IUserRepository.cs",
      "/src/Repositories/UserRepository.cs"
    ]
  },
  "dependencies": ["TASK-003", "TASK-006"],
  "verificationCriteria": [
    "Repository implements IUserRepository",
    "All CRUD operations implemented",
    "Multi-tenant filtering applied"
  ],
  "estimatedMinutes": 30
}
```

## No Tasks Available

If no tasks are available:
```json
{
  "available": false,
  "reason": "All queued tasks have unmet dependencies",
  "waitingFor": ["TASK-015"],
  "suggestion": "Complete TASK-015 to unblock 3 tasks"
}
```

## Error Handling

- File lock conflicts are automatically resolved
- Database is backed up before each update
- Failed updates trigger automatic rollback

## Integration with Agents

When called with --agent parameter:
1. Records agent assignment
2. Creates agent-specific working directory
3. Provides agent with full context
4. Sets up monitoring for task progress