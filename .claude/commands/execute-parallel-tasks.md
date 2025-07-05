# Execute Parallel Tasks with Sub-Agents

## Purpose

Get multiple independent tasks from the queue, implement them concurrently using sub-agents, verify completion, and update status.

## Process

1. **Get Parallel Tasks**
   - Read .claude/task-system/task-status.json to count in-progress tasks
   - Calculate available slots (maxParallel - inProgress, max 4)
   - Get tasks from queue that have no interdependencies
   - If queue insufficient, auto-queue available tasks
   - Mark all selected tasks as "in_progress"

2. **Load Task Details**
   - Read full details for all selected tasks from .claude/task-system/master-tasks.json
   - Verify no file conflicts between tasks
   - Group by functional area for efficient implementation

3. **Execute Implementations Using Sub-Agents**
   - Launch up to 4 concurrent sub-agents using the Task tool
   - Each sub-agent receives:
     - Specific task details and requirements
     - Project context (conventions, patterns, specifications)
     - Isolated working context to prevent conflicts
     - Clear output format requirements

4. **Build Verification Phase**
   - Run build after each batch of tasks
   - Identify compilation errors
   - Automatically fix common issues

5. **Error Resolution Phase**
   - Fix build errors before marking tasks complete
   - Update implementations as needed
   - Re-verify build success

6. **Playwright tests**
   - If any of the changes in this batch resulted in any User Interface changes, see if there are any Playwright tests associated to this User Interface and update them accordingly if you need to.

7. **Build Validation**
   - Make sure the project builds and runs okay, fix any issues if there are any.

8. **Summary**
   - When all complete, give me a summary of all tasks completed, how many parallel cycles were used and how many sub agents have been used in this run.

   Sub-agent prompt template:

   ```
   You are a specialized implementation agent for task [TASK-ID].
   
   Task Details:
   - Name: [Task Name]
   - Description: [Task Description]
   - Files to create: [File List]
   
   Context:
   - Project: ASP.NET Core 9.x Identity Server
   - Conventions: Follow conventions in .claude/context/conventions.md
   - Specification: Refer to /spec/pet-service-search-specification.md
   
   Implementation requirements:
   - Generate complete code without TODOs or placeholders or mocks or stubs, or NotImplementedExceptions
   - Include all error handling and validation
   - Add XML documentation comments
   - Follow conventions in .claude/context/conventions.md
   - Ensure all interface members are fully implemented
   - Include appropriate logging statements
   
   Output Format:
   ## Task Completion Report
   - Task ID: [ID]
   - Status: [COMPLETED/FAILED]
   - Files Created: [List]
   - Verification: [PASS/FAIL]
   - Issues: [Any problems]
   - Code: [Full implementation for each file]
   ```

9. **Merge and Verify Results**

- Collect outputs from all sub-agents
- Verify no naming conflicts occurred
- Check all files created for each task
- Run any applicable verification
- Track success/failure per task

10. **Update Status**

- Mark successful tasks as "completed"
- Mark failed tasks as "failed" with reasons
- Update phase progress
- Queue newly available dependent tasks

11. **Report Results**

- Summary of all tasks attempted
- Success/failure status for each
- Files created per task
- Next available tasks

12. **Error Handling**

- If a sub-agent fails, mark its task as "failed"
- Continue processing other successful tasks
- Report all failures with detailed reasons
- Allow retry of failed tasks later

13. **Onto the next tasks**
- Find and execute  "/.claude/commands/execute-parallel-tasks.md"

## Selection Strategy

- Prefer tasks from same functional area
- Avoid tasks that modify same files
- Respect dependency chains
- Maximum efficiency within constraints

## Parallel Execution Rules

- Maximum 3 concurrent sub-agents
- Each agent works in isolated namespace
- No shared file modifications between agents
- Coordinate entity relationships after completion
- Sub-agents should not update task status directly

## Sub-Agent Coordination

- Main agent maintains control of task status updates
- Sub-agents focus solely on implementation
- Results are merged after all agents complete
- Conflict resolution handled by main agent

## Example Flow

1. Available slots: 4
2. Gets TASK-002, TASK-003, TASK-005 (all depend on TASK-001)
3. Launches 3 sub-agents concurrently:
   - Agent 1: TASK-002 (Creates nuget.config, Directory.Build.props)
   - Agent 2: TASK-003 (Creates Core project)
   - Agent 3: TASK-005 (Creates test projects)
4. Collects results from all agents
5. Verifies implementations and updates status
6. Queues TASK-004 (now unblocked), TASK-008 (now unblocked)

### Build Verification Process

```powershell
# After task batch completes:
dotnet build src/BuildingBlocks/MeAndMyDog.SharedKernel/MeAndMyDog.SharedKernel.csproj
dotnet build src/BuildingBlocks/MeAndMyDog.BlobStorage/MeAndMyDog.BlobStorage.csproj  
dotnet build src/API/MeAndMyDog.API/MeAndMyDog.API.csproj
dotnet build src/Web/MeAndMyDog.WebApp/MeAndMyDog.WebApp.csproj
```

### Common Build Error Fixes

1. **Missing Interface Members**
   - Detect: "does not implement interface member"
   - Fix: Add missing method implementations

2. **Namespace Issues**
   - Detect: "type or namespace name could not be found"
   - Fix: Add missing using statements

3. **Type Mismatches**
   - Detect: "cannot convert from X to Y"
   - Fix: Adjust parameter/return types

4. **Missing Dependencies**
   - Detect: "Could not load file or assembly"
   - Fix: Add NuGet package references

## Implementation Plan

### Process 4: Build Check

```
Run builds in order:
1. SharedKernel (base dependencies)
2. BlobStorage (depends on SharedKernel)
3. API (depends on both)
4. WebApp (depends on all)
```

### Process 5: Error Resolution

```
For each error:
1. Identify file and line
2. Determine error type
3. Apply appropriate fix
4. Re-verify build
```
