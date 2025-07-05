# Get Next E2E Testing Task

## Purpose
Retrieve the next available task from the E2E testing queue, prioritizing critical path items and dependency resolution.

## Usage
```
claude-code run .claude/commands/e2e-next-task.md
```

## Process

1. **Read Current Queue State**
   - Load e2e-testing-task-queue.json
   - Check current active tasks
   - Verify parallel execution limits

2. **Check Dependencies**
   - Load e2e-testing-dependencies.json
   - Identify tasks with resolved dependencies
   - Prioritize critical path tasks

3. **Select Next Task**
   - Respect priority ordering (critical > high > medium)
   - Consider estimated effort for balanced workload
   - Ensure phase gating rules

4. **Update Queue Status**
   - Move selected task from queued to in_progress
   - Update timestamps
   - Adjust parallel task counter

5. **Return Task Details**
   ```json
   {
     "taskId": "E2E-001",
     "title": "Install and configure Playwright framework",
     "phase": "PHASE-001",
     "priority": "critical",
     "estimatedHours": 2,
     "files": ["playwright.config.ts", "package.json"],
     "acceptanceCriteria": [...],
     "dependencies": [],
     "instructions": "Detailed implementation guidance"
   }
   ```

## Selection Algorithm

**Priority Order:**
1. Critical path tasks with no dependencies
2. High priority tasks in current phase
3. Medium priority tasks that unblock others
4. Parallel-friendly tasks

**Phase Gating:**
- Complete 80% of current phase before advancing
- Allow overlap for independent task streams
- Prioritize framework tasks before feature tests

## Output Format

```
🎯 **Next Task: E2E-001**
📋 **Title:** Install and configure Playwright framework
🏗️ **Phase:** PHASE-001 - Framework Setup & Core Infrastructure
⚡ **Priority:** Critical
⏱️ **Estimated:** 2 hours

**Files to Create:**
- playwright.config.ts
- package.json updates

**Acceptance Criteria:**
✅ Playwright installed via npm
✅ Basic configuration file created  
✅ Test directory structure established
✅ Sample test runs successfully

**Implementation Guide:**
[Detailed step-by-step instructions]

**Dependencies:** None (ready to start)
**Blocks:** E2E-002, E2E-003, E2E-029 (8 tasks waiting)
```

## Error Handling

- If queue is empty: Suggest queue population
- If all tasks blocked: Show dependency chain
- If parallel limit reached: Show current active tasks
- If phase gating blocks: Show phase completion status