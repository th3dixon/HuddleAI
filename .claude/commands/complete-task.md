# Complete Task

## Purpose
Mark a task as completed and update the task system accordingly.

## Usage
```
claude-code run .claude/commands/complete-task.md <task-id>
```

## Process

1. **Validate Task**
   - Verify task exists
   - Confirm task is IN_PROGRESS
   - Check caller is assigned agent (if applicable)

2. **Run Verification**
   - Execute verification criteria
   - Check all required files exist
   - Run any specified tests
   - Validate output format

3. **Update Task Status**
   ```json
   {
     "status": "COMPLETED",
     "completedAt": "2024-01-13T12:30:00Z",
     "completedBy": "agent-1",
     "verificationResults": {
       "passed": 5,
       "failed": 0,
       "details": [...]
     },
     "duration": 1800
   }
   ```

4. **Update Dependencies**
   - Find all tasks depending on this one
   - Check if their dependencies are now met
   - Move eligible tasks from PENDING to QUEUED

5. **Archive Completed Task**
   - Move to completed-tasks.json
   - Preserve full history
   - Update statistics

6. **Queue Next Tasks**
   - Identify newly unblocked tasks
   - Add to queue in priority order
   - Send notifications if configured

## Status Options

### COMPLETED
- All verification passed
- Files created successfully
- Ready for dependent tasks

### FAILED
- Verification failed
- Requires investigation
- May block dependent tasks

### BLOCKED
- External dependency
- Waiting for clarification
- Temporary hold

## Output

```json
{
  "taskId": "TASK-016",
  "previousStatus": "IN_PROGRESS",
  "newStatus": "COMPLETED",
  "duration": "30 minutes",
  "newlyQueued": [
    "TASK-019: Implement User Service",
    "TASK-020: Create User Controller"
  ],
  "statistics": {
    "totalCompleted": 16,
    "phaseProgress": "80%",
    "estimatedCompletion": "2 days"
  }
}
```

## Verification Criteria

Each task has specific verification criteria:
- File existence checks
- Code compilation
- Test execution
- Linting passes
- Documentation present

## Error Recovery

If completion fails:
1. Status remains IN_PROGRESS
2. Error details are logged
3. Agent can retry or escalate
4. Manual intervention available

## Side Effects

Completing a task may:
- Unblock multiple dependent tasks
- Complete a phase
- Trigger milestone notifications
- Update velocity metrics