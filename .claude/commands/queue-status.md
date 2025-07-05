# Queue Status Command

## Purpose
Display current task queue status and system overview.

## Usage
```
claude-code run .claude/commands/queue-status.md
```

## Output

### Summary Statistics
- **Total Tasks**: 65 tasks across 3 phases
- **Current Status**: 4 queued, 61 pending
- **Estimated Effort**: 189 hours total
- **Current Phase**: PHASE-1 (Foundation)

### Queue Overview
```
🚀 Ready to Execute (4 tasks):
┌─────────┬─────────────────────────────────────┬──────────┬───────┬─────────────┐
│ Task ID │ Title                               │ Priority │ Hours │ Dependencies│
├─────────┼─────────────────────────────────────┼──────────┼───────┼─────────────┤
│ CMS-001 │ Create CmsPages database migration  │ critical │   2   │ None        │
│ CMS-003 │ Create CmsWidgetTypes migration     │ critical │   2   │ None        │
│ CMS-010 │ Create CMS DTOs                     │ critical │   3   │ None        │
│ CMS-018 │ Install GrapesJS packages           │ critical │   2   │ None        │
└─────────┴─────────────────────────────────────┴──────────┴───────┴─────────────┘
```

### Phase Progress
```
📊 PHASE-1 (Foundation - Weeks 1-3):
Progress: ████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 4/30 tasks queued

📊 PHASE-2 (Advanced Features - Weeks 4-6):
Progress: ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 0/22 tasks ready

📊 PHASE-3 (Polish and Launch - Weeks 7-8):
Progress: ░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░ 0/13 tasks ready
```

### Next Actions
1. **Execute parallel tasks**: Run 4 queued tasks concurrently
2. **Monitor dependencies**: 7 tasks will unlock after current batch
3. **Phase completion**: 26 more tasks remain in PHASE-1

### Configuration
- **Max Parallel**: 4 tasks
- **Auto Queue**: Enabled
- **Auto Advance**: Disabled (manual phase transitions)

---

*Task system initialized from PROVIDER-CMS-SPECIFICATION.md*
*Last updated: 2025-01-13T15:30:00Z*