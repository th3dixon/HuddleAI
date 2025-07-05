# Retry Task

## Purpose
Retry a failed or blocked task by resetting its status and adding it back to the queue.

## Usage
```
claude-code run .claude/commands/retry-task.md <TASK-ID>
```

## Process
1. **Validate Task**
   - Verify task exists
   - Check task is in FAILED or BLOCKED status
   - Review failure reason

2. **Reset Task State**
   - Clear failure information
   - Reset attempt counter
   - Update status to QUEUED

3. **Check Dependencies**
   - Verify all dependencies still satisfied
   - Alert if dependencies have changed

4. **Add to Queue**
   - Place at front of queue for immediate retry
   - Update queue timestamp

5. **Log Retry Attempt**
   - Record retry reason
   - Track retry count
   - Note any configuration changes

## Output Format
```
Task TASK-XXX queued for retry
Previous status: FAILED
Failure reason: [reason]
Dependencies: All satisfied ✓
Retry attempt: #2
Added to queue position: 1
```

## Validation
- Task must be FAILED or BLOCKED
- Dependencies must be satisfied
- Maximum retry limit not exceeded (3 attempts)