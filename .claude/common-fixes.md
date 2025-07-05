# Common Fixes Reference Guide

## 🔧 Quick Fixes for Frequent Issues

### 1. PaymentMethodType Ambiguity
```csharp
// Add at top of file
using BusinessToolsPaymentMethodType = MeAndMyDog.API.Entities.BusinessTools.PaymentMethodType;

// Use in code
public BusinessToolsPaymentMethodType PaymentMethod { get; set; }
```

### 2. ApplicationUser Ambiguity
```csharp
// Use fully qualified name
MeAndMyDog.API.Entities.Identity.ApplicationUser user = ...
```

### 3. Missing Interface Methods
```csharp
// IPaymentService requires these:
public async Task<PaymentMethodValidationResult> ValidatePaymentMethodAsync(string userId, string paymentMethodId)
{
    // Implementation
}

public async Task<SubscriptionUpdateResult> UpdateSubscriptionAsync(string userId, int subscriptionId, SubscriptionUpdateRequest request)
{
    // Implementation
}
```

### 4. Get User ID from Claims
```csharp
// Use extension method
var userId = User.GetUserId();
var providerId = User.GetPetServiceProviderId();

// Don't use hardcoded IDs!
// BAD: var userId = "12345";
// GOOD: var userId = User.GetUserId();
```

### 5. Null Reference Warnings
```csharp
// For DTOs/ViewModels
public string? PropertyName { get; set; }  // Add ?

// For returns
return result ?? throw new InvalidOperationException("Result cannot be null");
```

### 6. Build Commands
```bash
# Quick check after changes
dotnet build src/API/MeAndMyDog.API/MeAndMyDog.API.csproj --no-incremental

# Full solution
dotnet build MeAndMyDog.sln

# Clean if having issues
dotnet clean && dotnet restore && dotnet build
```

### 7. Common Service Patterns
```csharp
// Validation pattern
if (string.IsNullOrEmpty(parameter))
{
    _logger.LogWarning("Invalid parameter");
    return new Result { Success = false, Message = "Invalid parameter" };
}

// Try-catch pattern
try
{
    // Logic here
    await _context.SaveChangesAsync();
    _logger.LogInformation("Operation successful");
    return result;
}
catch (Exception ex)
{
    _logger.LogError(ex, "Operation failed");
    throw;
}
```

### 8. Entity Patterns
```csharp
// Always update timestamps
entity.ModifiedDate = DateTime.UtcNow;

// Soft delete pattern
entity.IsDeleted = true;
entity.ModifiedDate = DateTime.UtcNow;

// Include related data
var result = await _context.Entities
    .Include(e => e.RelatedEntity)
    .FirstOrDefaultAsync(e => e.Id == id);
```

### 9. Async Patterns
```csharp
// Always use async/await
public async Task<Result> MethodAsync()  // Not Task without async

// ConfigureAwait(false) in libraries
await SomeMethodAsync().ConfigureAwait(false);

// Parallel execution
var tasks = items.Select(item => ProcessAsync(item));
await Task.WhenAll(tasks);
```

### 10. Testing Patterns
```bash
# Run specific tests
dotnet test --filter "FullyQualifiedName~PaymentService"

# With detailed output
dotnet test --logger "console;verbosity=detailed"
```

## 🚨 Never Do These
1. ❌ Hardcode user IDs or provider IDs
2. ❌ Skip null checks on reference types
3. ❌ Ignore build warnings (they often become errors)
4. ❌ Create new files unless absolutely necessary
5. ❌ Use .Result or .Wait() on async methods
6. ❌ Forget to update ModifiedDate on entities
7. ❌ Skip logging in catch blocks
8. ❌ Use DateTime.Now (use DateTime.UtcNow)

## 📋 Pre-Task Checklist
- [ ] Read current-context.md for session state
- [ ] Check task-execution-log.md for completed work
- [ ] Run initial build to ensure clean state
- [ ] Group related tasks for parallel execution

## 📋 Post-Task Checklist
- [ ] Run build verification
- [ ] Update task-execution-log.md
- [ ] Mark tasks complete in todo list
- [ ] Update current-context.md with new findings