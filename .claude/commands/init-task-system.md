# Initialize Task Queue System

## Purpose
Initialize a robust, repeatable task management system from a technical specification.

## Usage
```
claude-code run .claude/commands/init-task-system.md <path-to-spec>
```

## Process

1. **Read Technical Specification**
   - Parse the provided specification file
   - Extract all requirements and features
   - Identify major components and phases

2. **Create Task System Structure**
   ```
   mkdir -p .claude/task-system
   mkdir -p .claude/commands
   mkdir -p .claude/templates
   ```

3. **Generate Master Task List**
   - Break down spec into atomic tasks (1-2 files each)
   - Assign unique IDs (TASK-001 to TASK-NNN)
   - Define dependencies between tasks
   - Group into logical phases
   - Set priorities within phases

4. **Create Core Files**

   ### master-tasks.json
   Complete list of all tasks with full metadata

   ### task-queue.json
   ```json
   {
     "metadata": {
       "totalTasks": 100,
       "createdAt": "2024-01-13T10:00:00Z",
       "lastUpdated": "2024-01-13T10:00:00Z"
     },
     "queue": [],
     "settings": {
       "maxParallel": 3,
       "autoQueue": true
     }
   }
   ```

   ### task-status.json
   ```json
   {
     "summary": {
       "total": 100,
       "completed": 0,
       "inProgress": 0,
       "queued": 0,
       "pending": 100,
       "failed": 0,
       "blocked": 0
     },
     "tasks": {}
   }
   ```

   ### task-dependencies.json
   Dependency graph for efficient queue management

5. **Initialize First Phase**
   - Move Phase 1 tasks from PENDING to QUEUED
   - Tasks with no dependencies are immediately available
   - Update task-queue.json with initial tasks

6. **Create Helper Commands**
   - next-task.md
   - complete-task.md
   - queue-status.md
   - parallel-tasks.md
   - retry-task.md

7. **Validation**
   - Ensure all tasks have unique IDs
   - Verify dependency graph has no cycles
   - Check that all phases are properly ordered
   - Validate file paths are consistent

## Output

After initialization, you'll have:
- Complete task system ready for execution
- First tasks queued and ready
- All dependencies mapped
- Status tracking initialized

Run `claude-code run .claude/commands/queue-status.md` to see the initial state.

## Example Task Generation

From spec requirement: "User authentication with MFA"

Generated tasks:
```
TASK-021: Create User entity model
TASK-022: Create authentication service interface  
TASK-023: Implement password hashing service
TASK-024: Create login controller and views
TASK-025: Implement session management
TASK-026: Create MFA service interface
TASK-027: Implement TOTP provider
TASK-028: Create MFA setup UI
TASK-029: Implement MFA verification
TASK-030: Create authentication tests
```

Each task is atomic, has clear dependencies, and creates 1-2 files maximum.