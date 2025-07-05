# Coding Conventions - ident-server-v3

## C# Naming Conventions

### General Rules
- **PascalCase**: Classes, methods, properties, events, namespaces
- **camelCase**: Parameters, local variables, private fields (with underscore prefix)
- **UPPER_CASE**: Constants
- **Explicit Variable Declarations**: All variable type declarations should be explicit. No use of var.
- Avoid generic names
- Use domain-specific names, avoid generic terms
- Follow BEM for CSS classes
- Use UK spelling for names (i.e. Organisation rather than Organization)

### Specific Patterns
- Interfaces: Prefix with 'I' (e.g., IUserService)
- Services: Suffix with 'Service' (e.g., UserService)
- Repositories: Suffix with 'Repository' 
- DTOs: Suffix with 'Dto' (e.g., UserDto)
- Requests: Suffix with 'Request' (e.g., CreateUserRequest)
- Responses: Suffix with 'Response' (e.g., UserResponse)
- Validators: Suffix with 'Validator' (e.g., UserValidator)
- Enums: Suffix with 'Type' (e.g., 'ResponseType')
- Enum Values: All enum values should start from the value 1 (not 0)

### Async Methods
- Suffix with 'Async' (e.g., GetUsersAsync())
- Always return Task or Task<T>

## Database Conventions
- Use INT IDENTITY for primary keys
- Add Guid column for external references
- Only Use NVARCHAR(MAX) for JSON configuration fields that Require it, otherwise use fixed length NVARCHAR fields
- Create indexes for all foreign keys and frequently queried fields

### Tables
- PascalCase plural (e.g., Users, Products)
- No prefixes (no 'tbl_')
- Junction tables: Entity1Entity2 (e.g., UserRoles)

### Columns
- PascalCase (e.g., FirstName, CreatedDate)
- Primary keys: EntityId (not Id)
- Foreign keys: EntityId (e.g., UserId)
- Timestamps: CreatedAt, UpdatedAt

### Indexes
- Pattern: IX_TableName_Column1_Column2
- Unique: UX_TableName_Column

## File Organization

### Structure
- One class per file
- File name matches class name exactly
- Organize by feature, not by type

### Security Considerations
- Always use parameterized queries
- Implement rate limiting on all endpoints
- Enable security headers (HSTS, CSP, etc.)
- Field-level encryption for sensitive data
- Audit log all administrative actions

### Folder Structure
```
/Features
  /Users
    /Controllers
    /Services
    /Models
    /Validators
  /Products
    /Controllers
    /Services
    /Models
```

## API Conventions

### Routes
- RESTful resource naming
- Plural for collections: /api/users
- Singular for single resource: /api/users/{id}
- Nested resources: /api/users/{userId}/orders

### HTTP Methods
- GET: Retrieve data
- POST: Create new resource
- PUT: Update entire resource
- PATCH: Partial update
- DELETE: Remove resource

### Response Codes
- 200 OK: Successful GET, PUT
- 201 Created: Successful POST
- 204 No Content: Successful DELETE
- 400 Bad Request: Validation errors
- 401 Unauthorized: Not authenticated
- 403 Forbidden: Not authorized
- 404 Not Found: Resource doesn't exist

## Comments and Documentation

### XML Documentation
- All public APIs must have XML comments
- Include: summary, parameters, returns, exceptions

### Inline Comments
- Explain "why", not "what"
- Complex business logic only
- TODOs must include date and author

## Key Architectural Decisions

### Primary Key Strategy
- Integer primary keys for database entities
- GUID fields for API/UI exposure
- Never expose integer IDs in URLs or APIs

