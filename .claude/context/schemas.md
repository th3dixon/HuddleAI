# Database Schemas - ident-server-v3

## Naming Patterns
- Primary Keys: [Table]Id (int)
- Foreign Keys: [RelatedTable]Id
- Soft Delete: IsDeleted, DeletedAt

## Common Base Entity
```csharp
public abstract class BaseEntity
{
    public Guid GuidId { get; set; }
}
```

## Index Patterns
- Foreign Keys: IX_[Table]_[Column]
- Composite: IX_[Table]_[Column1]_[Column2]
- Unique: UX_[Table]_[Column]
- Includes: IX_[Table]_[Column]_Includes_[IncludedColumns]

## Common Configurations
```csharp
// Soft delete global filter
builder.HasQueryFilter(e => !e.IsDeleted);

// Audit fields
builder.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
builder.Property(e => e.UpdatedAt).HasDefaultValueSql("GETUTCDATE()");
```

## JSON Columns
- Use for: Complex types, lists, configuration data
- Configure with: 'builder.Property(e => e.Data).HasColumnType("nvarchar(max)")'
- Consider performance implications for large datasets

## Migration Strategy
1. Code-first migrations
2. Separate migration project
3. Seed data in migrations
4. Version control all migrations
5. Test rollback procedures
