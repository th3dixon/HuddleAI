# Get Next Task

## Purpose
Retrieve the next available task from the queue and mark it as in-progress.

## Usage
```
claude-code run .claude/commands/next-task.md
```

## Process

1. **Read Current State**
   - Load task-queue.json for available tasks
   - Load task-status.json for current progress
   - Load master-tasks.json for task details
   - Check maxParallel setting

2. **Select Task**
   - Pick first available task from queue
   - Verify all dependencies are completed
   - Check if under parallel execution limit
   - Prefer tasks from active phases

3. **Update Status**
   - Mark task as "in-progress"
   - Remove from queue
   - Update task-status.json counts
   - Record start timestamp

4. **Display Task Details**
   - Show task ID, title, and phase
   - List files to create/modify
   - Show estimated hours
   - Display description and context
   - Show related tasks

5. **Prepare Workspace**
   - Check if directories exist
   - List any related existing files
   - Show relevant documentation

## Output Format
```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
📋 NEXT TASK: TASK-001
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Title: Create Payment Service Interface
Phase: PHASE-001 - Critical Payment Integration
Type: backend
Priority: 1
Estimated: 2 hours

📁 Files to create:
  • Services/Interfaces/IPaymentService.cs

📝 Description:
Define IPaymentService interface with methods for subscription and token purchases

✅ Dependencies: None
🔗 Related tasks: TASK-002, TASK-004

💡 Implementation hints:
- Include methods for subscription creation, renewal, cancellation
- Add token purchase and balance management methods
- Consider webhook handling interface methods
- Include proper async/await patterns

Current Progress: 0/85 tasks completed (0%)
In Progress: 1 | Queued: 6 | Blocked: 0
```

## Error Handling
- **No tasks in queue**: Check for blocked tasks or advance phases
- **Max parallel reached**: Show current in-progress tasks
- **All dependencies blocked**: Display dependency chain
- **Phase not active**: Suggest activating next phase