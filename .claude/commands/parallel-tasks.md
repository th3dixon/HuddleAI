# Execute Parallel Tasks Command

## Purpose
Execute multiple tasks in parallel using sub-agents for maximum efficiency.

## Usage
```
claude-code run .claude/commands/parallel-tasks.md
```

## Process

### 1. Queue Validation
- Check current queue status
- Verify task dependencies are met
- Ensure no conflicting file modifications

### 2. Task Assignment
**Current Queue (4 tasks ready):**

#### Task CMS-001: Create CmsPages database migration
- **Agent**: Database Migration Specialist
- **Files**: `src/API/MeAndMyDog.API/Data/Migrations/*_CreateCmsPagesTable.cs`
- **Priority**: Critical
- **Estimated**: 2 hours

#### Task CMS-003: Create CmsWidgetTypes database migration  
- **Agent**: Database Schema Designer
- **Files**: `src/API/MeAndMyDog.API/Data/Migrations/*_CreateCmsWidgetTypesTable.cs`
- **Priority**: Critical
- **Estimated**: 2 hours

#### Task CMS-010: Create CMS DTOs
- **Agent**: API Model Developer
- **Files**: 
  - `src/API/MeAndMyDog.API/Models/Cms/CmsPageDto.cs`
  - `src/API/MeAndMyDog.API/Models/Cms/CmsWidgetDto.cs`
  - `src/API/MeAndMyDog.API/Models/Cms/CreateCmsPageRequest.cs`
  - `src/API/MeAndMyDog.API/Models/Cms/UpdateCmsPageRequest.cs`
- **Priority**: Critical
- **Estimated**: 3 hours

#### Task CMS-018: Install and configure GrapesJS packages
- **Agent**: Frontend Package Manager
- **Files**:
  - `src/Web/MeAndMyDog.WebApp/package.json`
  - `src/Web/MeAndMyDog.WebApp/vite.config.js`
- **Priority**: Critical
- **Estimated**: 2 hours

### 3. Parallel Execution
```
🚀 Launching 4 concurrent agents...

┌─────────┬──────────────────┬─────────────┬──────────┐
│ Agent   │ Task            │ Status      │ Progress │
├─────────┼──────────────────┼─────────────┼──────────┤
│ Agent-1 │ CMS-001         │ In Progress │ ████░░░░ │
│ Agent-2 │ CMS-003         │ In Progress │ ██████░░ │
│ Agent-3 │ CMS-010         │ In Progress │ ███░░░░░ │
│ Agent-4 │ CMS-018         │ In Progress │ █████░░░ │
└─────────┴──────────────────┴─────────────┴──────────┘
```

### 4. Completion Validation
After all agents complete:
- ✅ Verify all acceptance criteria met
- ✅ Run build validation
- ✅ Update task status to completed
- ✅ Queue next available tasks
- ✅ Generate completion report

### 5. Next Queue Population
**Tasks unlocked after completion:**
- CMS-002: Create CmsWidgets migration (depends on CMS-001)
- CMS-004: Create CmsPageViews migration (depends on CMS-001)  
- CMS-005: Create CmsPageBookings migration (depends on CMS-001)
- CMS-006: Create CmsPage entity model (depends on CMS-001)
- CMS-008: Create CmsWidgetType entity model (depends on CMS-003)
- CMS-011: Create ICmsPageService interface (depends on CMS-010)
- CMS-013: Create IWidgetService interface (depends on CMS-010)
- CMS-019: Create GrapesJS Vue component wrapper (depends on CMS-018)

**Next parallel batch (8 tasks) will be queued automatically.**

### Error Handling
- If any task fails, agents will retry once
- Build errors will halt queue progression
- Failed tasks marked for manual review
- Dependency chain validates before proceeding

### Performance Metrics
- **Target completion**: 4 tasks in 45-60 minutes
- **File conflicts**: 0 (verified non-overlapping)
- **Dependency validation**: ✅ All clear
- **Build impact**: Minimal (migrations and DTOs)

---

**Ready to execute? All 4 tasks are independent and can run safely in parallel.**