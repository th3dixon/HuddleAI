# Execute Next Task

## Purpose
Get the next available task from the queue, implement it completely, verify completion, and update status.

## Process

1. **Get Next Task**
   - Read .claude/task-system/task-status.json to check for in-progress tasks
   - If no in-progress task, get first task from queue
   - If queue empty, auto-queue available tasks from current phase
   - Mark selected task as "in_progress"

2. **Load Task Details**
   - Read full task details from .claude/task-system/master-tasks.json
   - Get list of files to create/modify
   - Review dependencies to ensure they're met

3. **Execute Implementation**
   - Create all required directories
   - Implement all files specified in the task
   - Follow project conventions and patterns
   - Use appropriate frameworks and libraries
   - Implementation requirements:
      - Generate complete code without TODOs or placeholders or mocks or stubs, or NotImplementedExceptions
      - Include all error handling and validation
      - Add XML documentation comments
      - Follow conventions in .claude/context/conventions.md
      - Ensure all interface members are fully implemented
      - Include appropriate logging statements

4. **Verify Completion**
   - Check all specified files were created
   - Ensure code compiles (if applicable)
   - Verify implementation matches requirements

5. **Update Status**
   - Mark task as "completed" in .claude/task-system/task-status.json
   - Remove from queue if still present
   - Update phase progress
   - Queue newly available dependent tasks

6. **Report Results**
   - Show what was implemented
   - List files created/modified
   - Display next available tasks

## Error Handling
- If implementation fails, mark task as "failed" with reason
- Provide clear error messages for troubleshooting
- Allow retry with `retry-task.md`

## Example Flow
1. Gets TASK-001 from queue
2. Creates solution and project files
3. Verifies files exist and are valid
4. Marks TASK-001 complete
5. Queues TASK-002, TASK-003, TASK-005, TASK-006, TASK-007