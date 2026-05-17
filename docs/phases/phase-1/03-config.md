# Phase 3 — Configuration Layer (MongoDB)

## Goal

Tenant-scoped business rules and resource metadata stored in MongoDB. The Application layer reads config through a typed, cached interface and never constructs MongoDB queries directly.

---

## Technical Specification

### 1. Current state

The following is already implemented and should not be changed:

| Component | Location | Status |
|---|---|---|
| `IConfigProvider<T>` | `Slot.Config/Interfaces/` | Done |
| `CachedConfigProvider<T>` | `Slot.Config.Cached/Providers/` | Done |
| `ICache` / `MemoryCache` | `Slot.Cache` / `Slot.Cache.Memory` | Done |
| `TenantConfigDocument` | `Slot.Infrastructure/Documents/` | Done |
| `ResourceConfigDocument` | `Slot.Infrastructure/Documents/` | Done |
| `TenantConfig` value object | `Slot.Infrastructure/ValueObjects/` | Done |
| `TenantConfigProvider` | `Slot.Infrastructure/Providers/` | Done |
| `ResourceConfigProvider` | `Slot.Infrastructure/Providers/` | Done |
| `ITenantConfigProvider` | `Slot.Infrastructure/Interfaces/` | Done |
| `IResourceConfigProvider` | `Slot.Infrastructure/Interfaces/` | Done |
| `Bootstrap.AddConfig(...)` | `Slot.Infrastructure/Bootstrap.cs` | Done |

---

### 2. Add write operations — `IConfigProvider` extension

Currently `IConfigProvider<T>` only has `GetAsync`. Add `UpsertAsync` for document creation and updates.

**`Slot.Config/Interfaces/IConfigProvider.cs`**:

```csharp
public interface IConfigProvider<TConfig> where TConfig : class
{
    Task<TConfig> GetAsync(string configReference, CancellationToken cancellationToken = default);
    Task UpsertAsync(string configReference, TConfig config, CancellationToken cancellationToken = default);
}
```

**`CachedConfigProvider<T>`** — invalidate cache on upsert:

```csharp
public async Task UpsertAsync(string configReference, TConfig config, CancellationToken cancellationToken = default)
{
    await configProvider.UpsertAsync(configReference, config, cancellationToken);
    await cache.RemoveAsync(configReference);  // force reload on next Get
}
```

Add `RemoveAsync(object key)` to `ICache` and implement in `MemoryCache`.

**`TenantConfigProvider`** — implement `UpsertAsync`:

```csharp
protected override async Task UpsertDocumentAsync(string configReference, TenantConfig config, CancellationToken cancellationToken)
{
    var doc = Map(config);  // TenantConfig → TenantConfigDocument
    var filter = GetExpression(configReference);
    await collection.ReplaceOneAsync(filter, doc, new ReplaceOptions { IsUpsert = true }, cancellationToken);
}
```

Same for `ResourceConfigProvider`.

---

### 3. Create default documents from use cases

When a `Tenant` is created (Phase 4), the use case must:

```csharp
await tenantConfigProvider.UpsertAsync(tenant.ConfigId, TenantConfig.Default, cancellationToken);
```

When a `Resource` is created:

```csharp
await resourceConfigProvider.UpsertAsync(resource.ConfigReference, ResourceConfig.Default, cancellationToken);
```

This ensures every entity always has a corresponding config document.

---

### 4. `ResourceConfig` — add `Default`

Mirror the pattern from `TenantConfig.Default`:

```csharp
public sealed class ResourceConfig
{
    public static readonly ResourceConfig Default = new();
    public string ResourceType { get; init; } = "Staff";
    public Dictionary<string, string> Metadata { get; init; } = [];
}
```

---

### 5. Application layer interface

Expose config providers to the Application layer through an interface defined in `Slot.Application`. The Application layer depends on the interface, not on `Slot.Infrastructure`.

**`Slot.Application/Interfaces/Config/ITenantConfigReader.cs`**:

```csharp
public interface ITenantConfigReader
{
    Task<TenantConfig> GetAsync(string configId, CancellationToken cancellationToken = default);
}
```

**`Slot.Application/Interfaces/Config/ITenantConfigWriter.cs`**:

```csharp
public interface ITenantConfigWriter
{
    Task UpsertAsync(string configId, TenantConfig config, CancellationToken cancellationToken = default);
}
```

Same pair for `IResourceConfigReader` / `IResourceConfigWriter`.

Implementations in `Slot.Infrastructure` are thin adapters over `ITenantConfigProvider`:

```csharp
internal sealed class TenantConfigReader(ITenantConfigProvider provider) : ITenantConfigReader
{
    public Task<TenantConfig> GetAsync(string configId, CancellationToken ct)
        => provider.GetAsync(configId, ct);
}
```

Register in `Bootstrap.AddConfig(...)`.

---

### 6. Done when

- [ ] `IConfigProvider<T>` has `UpsertAsync`
- [ ] `ICache` has `RemoveAsync`; `MemoryCache` implements it
- [ ] `CachedConfigProvider` invalidates cache on upsert
- [ ] `TenantConfigProvider` and `ResourceConfigProvider` implement `UpsertAsync`
- [ ] `ResourceConfig.Default` exists
- [ ] `ITenantConfigReader`, `ITenantConfigWriter`, `IResourceConfigReader`, `IResourceConfigWriter` defined in `Slot.Application`
- [ ] Adapters registered in `Slot.Infrastructure/Bootstrap.cs`
- [ ] Integration tests: upsert a config document, read it back, verify cache invalidation

---

## Documentation

### What is the Configuration Layer?

This layer stores per-tenant business rules and resource metadata in MongoDB. It is separate from PostgreSQL because:

- Config changes frequently and is read-heavy — caching fits naturally
- Config is schema-flexible — each resource type can carry different metadata
- Keeping config out of the relational model prevents table proliferation

The config layer answers questions like: *"How many hours before a slot can a customer cancel?"*, *"Is credit pack freezing enabled for this tenant?"*, *"Is this resource a trainer or a room?"*

### Document structure

**`TenantConfigDocument`** — one document per tenant, referenced by `Tenant.ConfigId`:

```
{
  _id: "<ObjectId>",
  terminology: { customer, resource, serviceType, slot, creditPack },
  booking: { cancellationDeadlineHours, allowCustomerCancellation, creditReturnOnLateCancellation },
  creditPack: { enabled, freezingEnabled, expiryExtendedOnFreeze },
  slot: { requiresResource, allowGroupBookings }
}
```

**`ResourceConfigDocument`** — one document per resource, referenced by `Resource.ConfigReference`:

```
{
  _id: "<ObjectId>",
  resourceType: "Staff" | "Space",
  metadata: { key: value, ... }
}
```

### Caching strategy

```
Application use case
    → ITenantConfigReader (Application interface)
        → TenantConfigReader (Infrastructure adapter)
            → CachedConfigProvider<TenantConfig>
                → ICache (in-memory, TTL-based)
                → ITenantConfigProvider (MongoDB, fallback on cache miss)
```

The cache is keyed by `configReference` (the MongoDB `_id`). TTL is controlled by `CacheOptions.TtlSeconds`.

On `UpsertAsync`, the cache key is evicted immediately — the next `GetAsync` fetches a fresh document from MongoDB. This means config changes take effect on the next request after the update, with no stale reads.

### Why split Reader / Writer interfaces?

Use cases that only read config depend on `ITenantConfigReader`. Use cases that create tenants or resources depend on `ITenantConfigWriter`. This makes dependencies explicit and keeps read-only use cases lighter.

### `TenantConfig.Default` / `ResourceConfig.Default`

Static defaults ensure that a config document is always valid when first created, without requiring the caller to construct a full object. Use cases call `UpsertAsync(id, TenantConfig.Default)` on tenant creation — the tenant can be customised later via an update endpoint.
