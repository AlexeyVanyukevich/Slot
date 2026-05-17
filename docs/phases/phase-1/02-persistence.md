# Phase 2 — Persistence Layer (PostgreSQL / EF Core)

## Goal

Map the domain model to a relational database. Repositories and `UnitOfWork` are the only exit points for reads and writes. The Application layer never touches `DbContext` or `DbSet` directly.

---

## Technical Specification

### 1. DbContext — add missing `DbSet`s

`Slot.Persistence/Contexts/AppDbContext.cs` — add explicit `DbSet` properties for all entities (EF discovers them via navigation, but explicit sets are better for queries):

```csharp
public DbSet<Tenant> Tenants => Set<Tenant>();
public DbSet<Customer> Customers => Set<Customer>();
public DbSet<Resource> Resources => Set<Resource>();
public DbSet<ServiceType> ServiceTypes => Set<ServiceType>();
public DbSet<SlotEntity> Slots => Set<SlotEntity>();
public DbSet<SlotResource> SlotResources => Set<SlotResource>();
public DbSet<Booking> Bookings => Set<Booking>();
public DbSet<CreditPack> CreditPacks => Set<CreditPack>();
```

---

### 2. Entity configurations — corrections and additions

**`SlotConfiguration.cs`** — remove `ResourceId` FK, add `SlotResource` many-to-many:

```csharp
// Remove:
builder.HasOne(x => x.Resource)...

// Add:
builder.HasMany(x => x.SlotResources)
    .WithOne(x => x.Slot)
    .HasForeignKey(x => x.SlotId)
    .OnDelete(DeleteBehavior.Cascade);

builder.HasIndex(x => new { x.TenantId, x.StartsAt });
```

**`SlotResourceConfiguration.cs`** — new file:

```csharp
internal sealed class SlotResourceConfiguration : IEntityTypeConfiguration<SlotResource>
{
    public void Configure(EntityTypeBuilder<SlotResource> builder)
    {
        builder.ToTable("slot_resources");
        builder.HasKey(x => new { x.SlotId, x.ResourceId });

        builder.HasOne(x => x.Resource)
            .WithMany(x => x.SlotResources)
            .HasForeignKey(x => x.ResourceId)
            .OnDelete(DeleteBehavior.Restrict);  // do not cascade-delete resource assignments
    }
}
```

**`BookingConfiguration.cs`** — add `CreditsConsumed` and `CancelReason` column mappings (EF auto-discovers properties, but explicit config confirms intent):

```csharp
builder.Property(x => x.CreditsConsumed).IsRequired();
builder.Property(x => x.CancelReason).HasMaxLength(512);
builder.HasIndex(x => new { x.SlotId, x.Status });
```

**`CustomerConfiguration.cs`** — add `ExternalAuthId`:

```csharp
builder.Property(x => x.ExternalAuthId).HasMaxLength(256);
builder.HasIndex(x => new { x.TenantId, x.Phone }).IsUnique();
builder.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
```

**`CreditPackConfiguration.cs`** — add index for active pack lookup:

```csharp
builder.HasIndex(x => new { x.CustomerId, x.IsFrozen, x.ValidUntil });
```

---

### 3. `SoftDeletableInterceptor` — add global query filter

Currently the interceptor converts deletes to `IsDeleted = true`. Add a global query filter so soft-deleted entities are excluded from all queries automatically:

In `AppDbContext.OnModelCreating`:

```csharp
foreach (var entityType in modelBuilder.Model.GetEntityTypes())
{
    if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
    {
        modelBuilder.Entity(entityType.ClrType)
            .HasQueryFilter(BuildSoftDeleteFilter(entityType.ClrType));
    }
}
```

Helper:
```csharp
private static LambdaExpression BuildSoftDeleteFilter(Type type)
{
    var param = Expression.Parameter(type);
    var prop = Expression.Property(param, nameof(ISoftDeletable.IsDeleted));
    var body = Expression.Equal(prop, Expression.Constant(false));
    return Expression.Lambda(body, param);
}
```

---

### 4. Repository interfaces (defined in Application layer, implemented here)

Interfaces live in `Slot.Application/Interfaces/Repositories/` — implementations in `Slot.Persistence/Repositories/`.

**`IUnitOfWork`**

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

**`ISlotRepository : IRepository<Slot, int>`**

```csharp
Task<List<int>> FindConflictingSlotIdsAsync(
    int tenantId,
    IReadOnlyList<int> resourceIds,
    DateTimeOffset startsAt,
    DateTimeOffset endsAt,
    int? excludeSlotId = null,
    CancellationToken cancellationToken = default);
```

Used to detect resource conflicts before creating or modifying a slot.

**`IBookingRepository : IRepository<Booking, int>`**

```csharp
Task<int> CountConfirmedAsync(int slotId, CancellationToken cancellationToken = default);

Task<List<Booking>> GetConfirmedBySlotAsync(int slotId, CancellationToken cancellationToken = default);
```

**`ICreditPackRepository : IRepository<CreditPack, int>`**

```csharp
// Returns packs ordered by ValidUntil ASC (soonest-expiring first)
Task<List<CreditPack>> GetActivePacksAsync(
    int customerId,
    CancellationToken cancellationToken = default);
```

Active = `IsFrozen = false AND ValidUntil > now AND UsedCredits < TotalCredits`.

---

### 5. `UnitOfWork` implementation

`Slot.Persistence/UnitOfWork.cs`:

```csharp
internal sealed class UnitOfWork(AppDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
```

Register as `Scoped` — one per HTTP request.

---

### 6. Bootstrap — register new services

Add to `Slot.Persistence/Bootstrap.cs`:

```csharp
services.AddScoped<IUnitOfWork, UnitOfWork>();
services.AddScoped<ISlotRepository, SlotRepository>();
services.AddScoped<IBookingRepository, BookingRepository>();
services.AddScoped<ICreditPackRepository, CreditPackRepository>();
```

---

### 7. Migration

Generate after all configurations are in place:

```bash
dotnet ef migrations add InitialSchema \
  --project Slot.Persistence \
  --startup-project Slot.API
```

Review the generated migration SQL before applying — verify:
- `slot_resources` table with composite PK
- All unique indexes (tenant+phone, tenant+email, tenant+slug)
- No unintended cascade deletes on `slot_resources.resource_id`

---

### 8. Done when

- [ ] `SlotResource` configuration exists with composite PK
- [ ] `Slot` configuration no longer references `ResourceId`
- [ ] `Booking` configuration maps `CreditsConsumed` and `CancelReason`
- [ ] Soft-delete global query filter applied to `Customer`
- [ ] `IUnitOfWork`, `ISlotRepository`, `IBookingRepository`, `ICreditPackRepository` defined
- [ ] All repository implementations use the existing `Repository<TEntity, TKey>` base
- [ ] Migration applies cleanly to a local PostgreSQL instance
- [ ] Integration tests: create slot with resources, find conflict, book, cancel

---

## Documentation

### What is the Persistence Layer?

The Persistence layer translates between the domain model and the database. It has two responsibilities:
1. **Mapping** — telling EF Core how to store entities (column names, constraints, indexes, relationships)
2. **Querying** — specialised read operations that cannot be expressed through the generic repository

Everything else (business rules, orchestration) belongs to Application or Domain.

### Repository pattern

The existing `Repository<TEntity, TKey>` base provides generic CRUD. Specialised repositories extend it with domain-specific queries:

```
IRepository<T, K>          — generic: GetById, Add, Update, Delete, GetAsync, GetSingleAsync
ISlotRepository            — extends with: FindConflictingSlotIdsAsync
IBookingRepository         — extends with: CountConfirmedAsync, GetConfirmedBySlotAsync
ICreditPackRepository      — extends with: GetActivePacksAsync
```

Use the generic `IEntityRepositoryFactory` for entities that only need basic CRUD (e.g. `Tenant`, `Resource`, `ServiceType`). Use the specialised interfaces where custom queries are needed.

### Unit of Work

`IUnitOfWork.SaveChangesAsync()` is the single commit point. Use cases orchestrate multiple repository calls and call `SaveChanges` exactly once at the end — this ensures all changes in a single business operation are either fully committed or fully rolled back.

A use case should never call `SaveChanges` in the middle of a business operation.

### Soft delete

`Customer` implements `ISoftDeletable`. The `SoftDeletableInterceptor` converts EF `Delete` operations to `IsDeleted = true` updates. The global query filter on `AppDbContext` ensures soft-deleted customers are invisible to all queries without any explicit `.Where(c => !c.IsDeleted)` at call sites.

### Interceptors

| Interceptor | Trigger | Effect |
|---|---|---|
| `AuditableInterceptor` | `SaveChanges` on Added entries | Sets `CreatedAt = now` via `TimeProvider` |
| `SoftDeletableInterceptor` | EF `Delete` on `ISoftDeletable` | Converts to `IsDeleted = true` update |

`TimeProvider` is injected into `AuditableInterceptor` — this makes time controllable in tests.

### Enums stored as strings

All enums (`SlotStatus`, `BookingStatus`, `CustomerStatus`) are stored as strings via `EnumToStringConverter<T>`. This makes the database readable without a lookup table and survives enum reordering in C#. The `HasMaxLength` on each enum column bounds the column size.

### Conflict detection query

`FindConflictingSlotIdsAsync` finds slots that share at least one resource and have an overlapping time window:

```sql
SELECT DISTINCT s.id
FROM slots s
JOIN slot_resources sr ON sr.slot_id = s.id
WHERE s.tenant_id = @tenantId
  AND sr.resource_id = ANY(@resourceIds)
  AND s.status = 'Scheduled'
  AND s.starts_at < @endsAt
  AND (s.starts_at + interval '1 minute' * st.duration_minutes) > @startsAt
  AND (@excludeSlotId IS NULL OR s.id <> @excludeSlotId)
```

The interval overlap condition is `a.start < b.end AND a.end > b.start`.
