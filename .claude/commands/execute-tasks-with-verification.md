# Execute Tasks with Build Verification

## Enhanced Workflow

1. **Task Execution Phase**
   - Execute up to 4 parallel tasks
   - Group related tasks for better efficiency
   - Track implementation results

2. **Build Verification Phase** 
   - Run build after each batch of tasks
   - Identify compilation errors
   - Automatically fix common issues

3. **Error Resolution Phase**
   - Fix build errors before marking tasks complete
   - Update implementations as needed
   - Re-verify build success

## Efficiency Improvements

### Task Selection Strategy
```
Priority Order:
1. Tasks with no dependencies (can run in parallel)
2. Tasks in same functional area (reduce context switching)
3. Tasks that create new files (avoid conflicts)
4. Tasks from active phases only
```

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

### Phase 1: Task Execution (4 parallel)
```
Select 4 tasks:
- Prefer independent tasks
- Group by feature area
- Avoid file conflicts
```

### Phase 2: Build Check
```
Run builds in order:
1. SharedKernel (base dependencies)
2. BlobStorage (depends on SharedKernel)
3. API (depends on both)
4. WebApp (depends on all)
```

### Phase 3: Error Resolution
```
For each error:
1. Identify file and line
2. Determine error type
3. Apply appropriate fix
4. Re-verify build
```

## Benefits

1. **Higher Success Rate** - Tasks only marked complete when they build
2. **Faster Development** - 33% more parallel capacity (3→4 tasks)
3. **Fewer Accumulated Errors** - Issues fixed immediately
4. **Better Task Grouping** - Related tasks done together
5. **Automated Fixes** - Common errors resolved automatically

## Usage

```
/execute-tasks-with-verification [batch-size]
```

This will:
1. Select optimal tasks (up to 4)
2. Execute in parallel
3. Verify builds
4. Fix any errors
5. Mark tasks complete only after successful build