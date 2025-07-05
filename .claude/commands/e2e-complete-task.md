# Complete E2E Testing Task

## Purpose
Mark a task as completed, update dependencies, and automatically queue next available tasks.

## Usage
```
claude-code run .claude/commands/e2e-complete-task.md <task-id>
```

## Arguments
- `task-id`: The task identifier (e.g., E2E-001)

## Process

1. **Validate Task Completion**
   - Verify task exists and is in progress
   - Check that all acceptance criteria are met
   - Validate created files exist

2. **Update Task Status**
   - Change status from in_progress to completed
   - Record completion timestamp
   - Update estimated vs actual hours

3. **Resolve Dependencies**
   - Load e2e-testing-dependencies.json
   - Find all tasks that depended on this task
   - Check if any dependent tasks are now ready

4. **Auto-Queue Ready Tasks**
   - Identify newly available tasks
   - Add to queue respecting priority and phase rules
   - Update queue counters

5. **Update Statistics**
   - Increment completed task count
   - Update phase progress percentages
   - Calculate velocity metrics

6. **Generate Report**
   ```
   ✅ **Task Completed: E2E-001**
   📊 **Phase Progress:** PHASE-001 (25% → 50%)
   🔓 **Unblocked Tasks:** E2E-002, E2E-003
   ⏭️ **Auto-Queued:** E2E-002 (critical priority)
   📈 **Velocity:** 2.5 tasks/day (↑15%)
   ```

## Validation Checks

**File Verification:**
- All specified files exist
- Files contain expected content structure
- No syntax errors in configuration files

**Acceptance Criteria:**
- Each criterion marked as complete
- Manual verification prompts for subjective criteria
- Integration test results if applicable

**Dependency Resolution:**
- Update dependency graph
- Verify no circular dependencies introduced
- Check for orphaned tasks

## Auto-Queue Logic

**Priority Rules:**
1. Critical tasks in current phase
2. Tasks that unblock the most other tasks
3. High priority tasks with all dependencies met
4. Tasks that advance the critical path

**Phase Advancement:**
- Allow next phase if current phase is 75% complete
- Prioritize foundational tasks across phases
- Maintain parallel task limits

## Error Handling

- **Task not found:** Show available in-progress tasks
- **Task not in progress:** Show current status
- **Missing files:** List expected vs actual files
- **Failed validation:** Show specific failure reasons
- **Dependency conflicts:** Show resolution suggestions

## Integration

Updates these files:
- e2e-testing-task-status.json
- e2e-testing-task-queue.json
- e2e-testing-dependencies.json (if needed)

Triggers:
- Automatic queuing of ready tasks
- Phase completion notifications
- Velocity recalculation
- Progress reporting