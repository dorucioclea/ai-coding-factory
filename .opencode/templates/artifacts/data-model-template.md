# Data Model: {PROJECT_NAME}

**Version**: 1.0
**Date**: {DATE}
**Author**: {Author}

---

## Overview

{1-2 sentences: What data does this system manage?}

---

## Entity Relationship Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                        CORE DOMAIN                               │
├─────────────────────────────────────────────────────────────────┤
│                                                                  │
│   ┌─────────────┐         ┌─────────────┐                       │
│   │    User     │─────1:N─│  {Entity}   │                       │
│   └─────────────┘         └──────┬──────┘                       │
│         │                        │                              │
│         │ 1:N                    │ 1:N                          │
│         ▼                        ▼                              │
│   ┌─────────────┐         ┌─────────────┐                       │
│   │   Session   │         │{SubEntity}  │                       │
│   └─────────────┘         └─────────────┘                       │
│                                                                  │
└─────────────────────────────────────────────────────────────────┘
```

---

## Entities

### User

**Description**: Registered users of the application

**Table**: `users`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `email` | VARCHAR(255) | UNIQUE, NOT NULL | Login email |
| `password_hash` | VARCHAR(255) | NOT NULL | Bcrypt hash |
| `display_name` | VARCHAR(100) | NOT NULL | Public display name |
| `avatar_url` | VARCHAR(500) | NULL | Profile image URL |
| `is_active` | BOOLEAN | DEFAULT true | Soft delete flag |
| `email_verified_at` | TIMESTAMP | NULL | When email was verified |
| `created_at` | TIMESTAMP | NOT NULL | Creation timestamp |
| `updated_at` | TIMESTAMP | NULL | Last update timestamp |

**Indexes**:
- `idx_users_email` UNIQUE on `email`
- `idx_users_is_active` on `is_active`

**C# Entity**:
```csharp
public class User : BaseEntity, IAggregateRoot
{
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string DisplayName { get; private set; }
    public string? AvatarUrl { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? EmailVerifiedAt { get; private set; }

    // Navigation
    public IReadOnlyCollection<Session> Sessions => _sessions.AsReadOnly();
    private readonly List<Session> _sessions = new();
}
```

---

### {Entity}

**Description**: {What this entity represents}

**Table**: `{entities}`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `name` | VARCHAR(200) | NOT NULL | {Description} |
| `description` | TEXT | NULL | {Description} |
| `created_by_id` | UUID | FK → users.id | Creator reference |
| `is_deleted` | BOOLEAN | DEFAULT false | Soft delete flag |
| `created_at` | TIMESTAMP | NOT NULL | Creation timestamp |
| `updated_at` | TIMESTAMP | NULL | Last update timestamp |

**Indexes**:
- `idx_{entities}_created_by` on `created_by_id`
- `idx_{entities}_is_deleted` on `is_deleted`

**Relationships**:
- Belongs to: User (via `created_by_id`)
- Has many: {SubEntity}

**C# Entity**:
```csharp
public class {Entity} : BaseEntity, IAggregateRoot
{
    public string Name { get; private set; }
    public string? Description { get; private set; }
    public Guid CreatedById { get; private set; }
    public bool IsDeleted { get; private set; }

    // Navigation
    public User CreatedBy { get; private set; }
    public IReadOnlyCollection<{SubEntity}> {SubEntities} => _{subEntities}.AsReadOnly();
    private readonly List<{SubEntity}> _{subEntities} = new();

    // Factory
    public static {Entity} Create(string name, string? description, Guid userId)
    {
        Guard.Against.NullOrWhiteSpace(name, nameof(name));

        return new {Entity}
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            CreatedById = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

---

### {SubEntity}

**Description**: {What this entity represents, typically a child of main entity}

**Table**: `{sub_entities}`

| Field | Type | Constraints | Description |
|-------|------|-------------|-------------|
| `id` | UUID | PK | Unique identifier |
| `{entity}_id` | UUID | FK → {entities}.id | Parent reference |
| `{field}` | {TYPE} | {constraints} | {Description} |
| `created_at` | TIMESTAMP | NOT NULL | Creation timestamp |

**Indexes**:
- `idx_{sub_entities}_{entity}_id` on `{entity}_id`

**Relationships**:
- Belongs to: {Entity}

---

## Value Objects

### Address (Example)

```csharp
public record Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string State { get; init; }
    public string PostalCode { get; init; }
    public string Country { get; init; }

    // Validation in constructor
    public Address(string street, string city, string state, string postalCode, string country)
    {
        Guard.Against.NullOrWhiteSpace(street, nameof(street));
        Guard.Against.NullOrWhiteSpace(city, nameof(city));
        // ...
    }
}
```

### Coordinate (Example)

```csharp
public record Coordinate
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }

    public Coordinate(double latitude, double longitude)
    {
        Guard.Against.OutOfRange(latitude, nameof(latitude), -90, 90);
        Guard.Against.OutOfRange(longitude, nameof(longitude), -180, 180);
    }
}
```

---

## Enumerations

### {EnumName}

```csharp
public enum {EnumName}
{
    Value1 = 0,
    Value2 = 1,
    Value3 = 2
}
```

**Database**: Stored as INT

---

## Relationships Summary

| Parent | Child | Type | FK Column | Cascade |
|--------|-------|------|-----------|---------|
| User | Session | 1:N | `user_id` | Delete |
| User | {Entity} | 1:N | `created_by_id` | Restrict |
| {Entity} | {SubEntity} | 1:N | `{entity}_id` | Delete |

---

## Common Patterns

### Soft Delete

All main entities use soft delete:

```csharp
public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    void Delete();
}
```

**Query Filter** (in DbContext):
```csharp
modelBuilder.Entity<{Entity}>()
    .HasQueryFilter(e => !e.IsDeleted);
```

### Auditing

All entities inherit audit fields:

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    public DateTime CreatedAt { get; protected set; }
    public DateTime? UpdatedAt { get; protected set; }
}
```

### Owned Types (Value Objects)

```csharp
modelBuilder.Entity<{Entity}>()
    .OwnsOne(e => e.Address, a =>
    {
        a.Property(p => p.Street).HasColumnName("address_street");
        a.Property(p => p.City).HasColumnName("address_city");
        // ...
    });
```

---

## Migrations

### Initial Migration

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Migration Naming

Format: `YYYYMMDD_Description`

Examples:
- `20240123_InitialCreate`
- `20240124_AddUserAvatar`
- `20240125_AddEntityTable`

---

## Seed Data

### Development Seeds

```csharp
public static class SeedData
{
    public static void Seed(ApplicationDbContext context)
    {
        if (!context.Users.Any())
        {
            context.Users.Add(new User
            {
                Email = "admin@example.com",
                DisplayName = "Admin User",
                // ...
            });
        }
        context.SaveChanges();
    }
}
```

---

## Performance Considerations

### Recommended Indexes

- All foreign keys
- Frequently filtered columns (`is_active`, `is_deleted`)
- Search columns (`name`, `email`)
- Date range queries (`created_at`)

### Pagination

All list queries should use keyset pagination:

```csharp
var query = context.Entities
    .Where(e => e.CreatedAt < lastCreatedAt)
    .OrderByDescending(e => e.CreatedAt)
    .Take(pageSize);
```

### Eager Loading

Define standard includes:

```csharp
public static IQueryable<{Entity}> WithStandardIncludes(this IQueryable<{Entity}> query)
{
    return query
        .Include(e => e.CreatedBy)
        .Include(e => e.{SubEntities});
}
```

---

## TypeScript Types (Frontend)

```typescript
// Generated from backend DTOs

interface User {
  id: string;
  email: string;
  displayName: string;
  avatarUrl?: string;
  createdAt: string;
}

interface {Entity} {
  id: string;
  name: string;
  description?: string;
  createdBy: User;
  createdAt: string;
}

interface {SubEntity} {
  id: string;
  {entity}Id: string;
  {field}: {type};
  createdAt: string;
}
```

---

## Changelog

| Date | Change | Migration |
|------|--------|-----------|
| {date} | Initial model | `InitialCreate` |
