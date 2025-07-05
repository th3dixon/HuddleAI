# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Current Project Status

**Active Implementation**: AI-powered sports analysis prototype with video/image upload functionality using Gemini 1.5 Flash API.

**Completed Features:**
- ✅ .NET Core 9 solution with n-tier architecture (API + Web)
- ✅ Gemini API integration service for sports performance analysis
- ✅ Domain entities and DTOs for analysis requests/responses
- ✅ RESTful API endpoints for video/image analysis
- ✅ Tailwind CSS-based web interface with AJAX file upload
- ✅ Progress indicators and real-time feedback
- ✅ Structured AI prompts for consistent analysis results

**Current Configuration:**
- API runs on: https://localhost:7010
- Web runs on: https://localhost:56680 (default port)
- Gemini API Key: Configured in appsettings.json
- Database: In-memory database for prototype (no persistence between restarts)

## 📁 Essential Files for Efficient Development

### Task Management
- **`.claude/task-system/master-tasks.json`** - Structured task data with phases and priorities (77 tasks across 10 phases)
- **`.claude/task-system/task-queue.json`** - Current task queue status
- **`.claude/task-system/task-status.json`** - Task completion tracking

### Context Tracking
- **`.claude/current-context.md`** - Session progress, pending tasks, known issues
- **`.claude/task-execution-log.md`** - Detailed task completion history
- **`.claude/common-fixes.md`** - Quick reference for frequent build errors

**Start each session by:**
1. Reading `.claude/task-system/master-tasks.json` for current priorities
2. Checking `.claude/task-system/task-status.json` for task completion status
3. Reviewing `.claude/current-context.md` for the latest project state
4. Checking `.claude/common-fixes.md` if encountering build errors

## Project Overview

**HuddleAI** is a comprehensive sports analytics website with heavy social features built with ASP.NET Core 9. The platform provides AI-powered performance analysis for athletes through video and image analysis using Gemini Flash, combined with social networking features that allow athletes to connect, share content, and compare their performance insights.

## Technology Stack

- **Framework**: ASP.NET Core 9.0 with C# 12 (nullable reference types enabled)
- **Database**: SQL Server with Entity Framework Core 9.0
- **Frontend**: ASP.NET Core MVC with Vue.js 3 components, Tailwind CSS, Alpine.js
- **Authentication**: ASP.NET Core Identity with JWT Bearer tokens
- **Storage**: Azure Blob Storage for media files
- **Caching**: Redis (StackExchange.Redis) with fallback to memory cache
- **External APIs**: Gemini 2.0 Flash (sports performance analysis), Google Maps, Santander Payment Gateway
- **Real-time**: SignalR for messaging and notifications
- **Testing**: xUnit with Moq for mocking
- **UI Framework**: Multiple layout systems (Bootstrap legacy, Tailwind CSS modern)

## Development Commands

```bash
# Build and run
dotnet restore HuddleAI.sln
dotnet build HuddleAI.sln
dotnet run --project src/API/HuddleAI.API              # API: https://localhost:7010
dotnet run --project src/Web/HuddleAI.WebApp           # Web: https://localhost:56680

# Database operations
dotnet ef migrations add <MigrationName> --project src/API/HuddleAI.API --startup-project src/API/HuddleAI.API
dotnet ef database update --project src/API/HuddleAI.API --startup-project src/API/HuddleAI.API

# Testing
dotnet test                                               # Run all tests
dotnet test --filter "FullyQualifiedName~PaymentService" # Run specific tests
dotnet test --logger "console;verbosity=detailed"        # Verbose output

# Clean and restore
./clean-and-restore.ps1                                   # Windows PowerShell
Get-ChildItem -Path . -Include bin,obj -Recurse | Remove-Item -Recurse -Force  # Linux/Mac

# Build verification (run after task completion)
./.claude/scripts/verify-build.ps1                       # Build all projects
./.claude/scripts/verify-build.ps1 -Project api          # Build API only
./.claude/scripts/verify-build.ps1 -FixErrors            # Attempt auto-fixes
```

## Task Execution Workflow

**Efficient Development Process:**
1. Execute up to 4 tasks in parallel (increased from 3)
2. After each batch, run build verification
3. Fix any build errors before marking tasks complete
4. Only proceed to next batch after successful build

**Benefits:**
- 33% faster task completion with 4 parallel agents
- Zero accumulated build errors
- Higher quality implementations
- Automated error resolution

## Architecture Overview

The solution follows **Clean Architecture** principles with a consolidated API approach (evolved from microservices for better performance):

```
HuddleAI.sln
├── src/
│   ├── API/HuddleAI.API/              # Consolidated API (all services)
│   │   ├── Controllers/                  # Feature-organized controllers
│   │   ├── Services/                     # Business logic layer
│   │   │   ├── Payment/                  # Payment processing (UnifiedPaymentService)
│   │   │   ├── Analytics/               # Sports performance analytics
│   │   │   ├── AI/                      # Gemini integration for video/image analysis
│   │   │   └── Messaging/               # SignalR real-time features
│   │   ├── Data/                        # EF Core DbContext & configurations
│   │   └── Entities/                    # Domain models
│   ├── Web/HuddleAI.WebApp/          # MVC frontend
│   └── BuildingBlocks/
│       ├── HuddleAI.SharedKernel/     # Shared DTOs, enums, utilities
│       └── HuddleAI.BlobStorage/      # Azure storage abstraction
└── tests/                               # Test projects mirroring src structure
```

### Key Architectural Patterns

1. **Service Layer Pattern**: All business logic in service classes with interfaces
2. **Repository Pattern**: Via Entity Framework Core DbContext
3. **Factory Pattern**: `IPaymentServiceFactory` for provider selection
4. **Background Services**: Video/image analysis processing, subscription renewal, payment retry
5. **Middleware Pipeline**: Security headers, performance logging, error handling

## Core Services & Dependencies

### Payment System
- `IPaymentService` → `UnifiedPaymentService` → `IPaymentServiceFactory`
- `IPaymentMethodService` handles stored payment methods
- Providers: Santander (primary), PayPal (secondary), Mock (development)

### Real-time Features
- `UnifiedMessageHub` (SignalR) for chat
- `IUnifiedMessagingService` for message orchestration
- `IOnlineStatusService` for presence tracking

### AI Integration
- `IAIAnalyticsService` → `GeminiApiClient` for sports performance analysis from videos/images
- Mock implementations for development (`MockGeminiApiClient`)

## Database Conventions

- **Primary Keys**: `{EntityName}Id` (e.g., `AthleteProfileId`)
- **Soft Deletes**: `IsDeleted` boolean flag on all entities
- **Timestamps**: `CreatedDate`, `ModifiedDate` on base entities
- **Navigation Properties**: Follow EF Core conventions
- **Indexes**: On foreign keys and commonly queried fields

## API Patterns

- **Versioning**: URL segment versioning (`/api/v1/...`)
- **Authentication**: JWT Bearer tokens with claims-based authorization
- **Error Handling**: ProblemDetails RFC 7807 format
- **Pagination**: `PagedResult<T>` with offset/limit
- **Response Caching**: ETags for GET endpoints

## Task Management System

The project uses a comprehensive task management system in `.claude/task-system/`:
- **`master-tasks.json`** - Primary task tracking file with 77 structured tasks across 10 phases
- **`task-queue.json`** - Active task queue for current work
- **`task-status.json`** - Track task completion status
- Tasks are organized by phases (PHASE-001 through PHASE-010)
- Current focus: Payment integration (PHASE-001) and AI sports analytics (PHASE-002)
- Reference these files at the start of each session for current priorities
- Update task status as work progresses

## Common Issues & Solutions

1. **IPaymentMethodService DI Error**: Service is registered in `ServiceCollectionExtensions.cs` line 66
2. **Missing EF Tools**: `dotnet tool install --global dotnet-ef`
3. **API Versioning**: Use `Asp.Versioning.Mvc` v8.1.0, not older packages - see README.md for correct Program.cs setup
4. **Blob Storage in Dev**: Automatically uses mock storage if no connection string
5. **Alpine.js Directives**: In Razor views, use `@@mouseenter` to escape @ symbols for Alpine.js
6. **Layout Selection**: Primary layout is `_Layout.cshtml` with Tailwind navigation - avoid creating new layouts
7. **PaymentMethodType Ambiguity**: Use type aliases like `BusinessToolsPaymentMethodType`
8. **Missing EntityFramework**: Add `using Microsoft.EntityFrameworkCore;` for `FirstOrDefaultAsync`

## External Integrations

- **Gemini 2.0**: Sports performance analysis from videos/images (API key in configuration)
- **Google Maps**: Location services (feature flag: `UseRealGoogleMapsApi`)
- **Santander**: Payment processing (sandbox credentials in dev)
- **Didit.me**: KYC verification (optional, disabled by default)

## Frontend Architecture

### Layout System Evolution
The project uses a **dual layout system** during modernization:
- **Legacy Bootstrap**: `_Layout.cshtml` - Traditional Bootstrap 5 navigation (NOT CURRENTLY ACTIVE)
- **Modern Tailwind**: `_NavigationMegaMenu.cshtml` - New Tailwind CSS + Alpine.js mega menu

### Multiple Layout Files
- **`_LayoutTailwind.cshtml` - ACTIVE LAYOUT** - This is the layout currently being used by the site
- `_Layout.cshtml` - Legacy layout with Tailwind mega menu navigation (not currently active)
- `_LayoutNew.cshtml`, `_LayoutRedesign.cshtml` - Design iterations
- `_AdminLayout.cshtml` - Admin-specific layout

**IMPORTANT**: When making CSS or layout changes, update `_LayoutTailwind.cshtml` as this is the active layout file.

### Navigation System
- **Mega Menu**: Hover-activated dropdowns with Analytics, Community, Account sections
- **Analytics Menu**: Performance analysis, video uploads, coaching tools
- **Community Menu**: Forum, Team features, Performance sharing (trending tags removed)
- **Account Menu**: Athlete profiles, analytics dashboard, payment methods
- **Responsive**: Mobile-first with Alpine.js interactivity

### Frontend Technologies
- **Tailwind CSS**: Utility-first styling framework
- **Alpine.js**: Lightweight reactive framework for navigation interactions
- **Vue.js 3**: Component-based widgets and interactive elements
- **SignalR**: Real-time messaging and notifications
- **Font Awesome**: Icon system throughout the application

## E2E Testing & Authentication

### Test Users Available for E2E Testing
**✅ CREATED IN DATABASE** - The following test users are available for authentication testing:

| Email | Password | Role | Purpose |
|-------|----------|------|---------|
| `testuser@example.com` | `TestUser123!` | User | Regular user authentication tests |
| `admin@example.com` | `AdminUser123!` | Admin | Admin functionality testing |
| `petowner@example.com` | `PetOwner123!` | PetOwner | Pet owner user journey tests |
| `provider@example.com` | `Provider123!` | ServiceProvider | Service provider workflow tests |
| `unverified@example.com` | `Unverified123!` | User (unverified) | Email verification flow tests |

### E2E Testing Commands
```bash
# Comprehensive authentication testing
cd tests/E2E && npx playwright test tests/e2e/auth/login.spec.ts

# Test specific authenticated pages
cd tests/E2E && npx playwright test tests/e2e/dashboard/ --project=chromium-stable

# Run all E2E tests with authentication
cd tests/E2E && npx playwright test --grep "@auth" --project=chromium-stable
```

### Authentication Test Fixtures
- Test users are defined in `tests/E2E/fixtures/test-data/user-fixtures.ts`
- Authentication helpers available in `tests/E2E/page-objects/auth/auth-helper.ts`
- Login page object at `tests/E2E/page-objects/auth/login-page.ts`

## Development Notes

- **Feature Flags**: Managed via `IFeatureManager` (e.g., `UseRealVeoApi`, `UseRealEmailService`)
- **Secure Configuration**: Sensitive data in Azure Key Vault or user secrets
- **Background Jobs**: Hosted services run video processing, payment retry, subscription renewal
- **Performance**: Redis caching enabled, response compression, lazy loading disabled
- **UI Consistency**: Use Tailwind classes for new components, maintain Bootstrap compatibility for legacy

## Important Instructions

### Core Development Principles
- **Edit, don't create**: Always prefer editing existing files over creating new ones
- **Minimal changes**: Do what has been asked; nothing more, nothing less
- **No proactive documentation**: Never create .md or README files unless explicitly requested
- **Layout consistency**: Use `_Layout.cshtml` with Tailwind navigation - don't create new layout files
- **Build verification**: Always run build checks after completing tasks
- **Task tracking**: Update todo lists and execution logs for transparency

### File Creation Guidelines
- **Views**: Only create if no similar view exists to modify
- **Controllers**: Only create for entirely new feature areas
- **Services**: Only create if no existing service can be extended
- **Entities**: Only create for genuinely new domain concepts