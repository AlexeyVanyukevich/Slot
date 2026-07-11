# Booking Engine — Technical Architecture Spec

Bridges `docs/booking-platform-spec.md` (business rules), `docs/db-spec-mvp.md` (schema), and `docs/asset-storage-spec.md` (storage) into a concrete module layout, following the layering already established by `UBP.Tenant` and `UBP.IAM`. No `UBP.Booking` module exists yet — this is the plan for building it.

---

## Part I. Module Topology

### Two services, one domain database

`UBP.IAM` is already a separate auth microservice with its own DB (`bookingplatform_auth`). Everything else — tenants, resources, bookings, notifications, assets — is **one domain**, sharing **one Postgres database**. Each module, however, keeps its **own `DbContext`** (Part II) rather than a single EF model spanning every module's entities — `db-spec-mvp.md`'s "one `DbContext` for the entire domain" note is satisfied at the database level (one connection string, one schema, one migration workflow per module), not by forcing a shared model across module boundaries.

New projects to add, mirroring the `UBP.Tenant` / `UBP.IAM` split of `Domain` / `Persistence` / `Application` / `API`:

```
UBP.Booking/
  UBP.Booking.Domain/        # Resource, Schedule, FieldSchema, Booking entities
  UBP.Booking.Persistence/   # IEntityTypeConfiguration<T> + repositories for the above
  UBP.Booking.Application/   # CQRS commands/queries, validation, business rules
  UBP.Booking.API/           # host: hosts Tenant + Booking + Notifications
                              # controllers; each module still owns its own DbContext (Part II)

UBP.Notifications/
  UBP.Notifications.Domain/       # TenantNotificationChannel
  UBP.Notifications.Persistence/
  UBP.Notifications.Application/  # channel CRUD + dispatch-on-event

UBP.Storage/
  UBP.Storage.Abstractions/  # IAssetStorage, upload policy types (mirrors UBP.Cache.Abstractions)
  UBP.Storage.Local/         # dev provider (filesystem)
  UBP.Storage.S3/            # prod provider — selected by config, not by code (asset-storage-spec.md)
```

`UBP.Booking.API` is the new host for the domain — **not** `UBP.IAM.API`. `UBP.Tenant` currently has no `API`/`Application` project of its own; those tenant-facing endpoints (registration, profile, notification-channel management) belong on this same host. The host just wires up each module's own `AddPersistence`/`AddApplication` registration side by side — it does not assemble a single shared `DbContext` out of them (Part II).

### Dependency direction

```
UBP.Core.Domain / UBP.Core.Persistence(.EF)
        ▲                    ▲                    ▲
        │                    │                    │
  UBP.Tenant.*         UBP.Booking.*       UBP.Notifications.*
        ▲                    │                    ▲
        │                    ├────────────────────┘  (via Application-layer
        │                    │                         abstraction, not Persistence)
        │                    ▼
        │              UBP.Storage.Abstractions
        │
        └── referenced by UBP.Booking.API only to call UBP.Tenant.Persistence's
            own AddPersistence registration alongside Booking's — Booking.Domain/
            Application never reference Tenant.Domain/Persistence directly, and
            there is no shared DbContext to compose (Part II).
```

`Booking` must not take a project reference on `Tenant.Persistence` or vice versa. The only things that cross the boundary are:
- a bare `tenant_id : Guid` column (denormalized, per Appendix rule 8 — no navigation property, no join across modules in code),
- the `INotificationDispatcher` abstraction (Part IV below), not a direct dependency on `TenantNotificationChannel`.

This keeps each module's entities and repositories ignorant of each other, even though they live in the same physical database — each module's `DbContext` only knows its own tables (Part II).

---

## Part II. Persistence Topology: One `DbContext` per Module

Rather than a single composition-root `DbContext` assembled from every module's assembly, **each module keeps its own internal `DbContext`**, exactly as `UBP.Tenant.Persistence.Contexts.AppDbContext` already does today — `internal sealed`, calling `ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)` for its own types only. `UBP.Booking.Persistence` and `UBP.Notifications.Persistence` each add their own `internal sealed BookingDbContext` / `NotificationsDbContext` following the identical pattern. No new composition root, no project reaching across module boundaries to assemble a shared EF model.

All of these `DbContext`s point at the **same physical Postgres database** (one connection string, one schema) — they just don't share a `DbContext` *object* or model. That has three consequences to design for up front:

1. **Distinct migration history tables.** By default every `DbContext` writes to `__EFMigrationsHistory`; two contexts against one database collide on that table. Each module's `Bootstrap.AddPersistence` sets its own via `optionsBuilder.MigrationsHistoryTable("__EFMigrationsHistory_Booking")` (`_Notifications`, etc.), so `dotnet ef migrations add`/`database update` stays scoped per module — no different from how Tenant's migrations are generated today, just with an explicit table name so a second module doesn't silently collide with it.
2. **Cross-module FK constraints are enforced at the DB, not in the EF model.** `bookings.tenant_id` still gets a real Postgres FK to `tenants.id` — added by hand in the Booking migration (`migrationBuilder.AddForeignKey(...)` against a table that migration's own `DbContext` doesn't own) — even though `Booking`'s EF model has no navigation property to `TenantEntity` and never will. EF doesn't need to know about a relationship for Postgres to enforce it.
3. **No cross-context joins in LINQ.** Anything needing data from two modules (e.g. "resource + its tenant's timezone") is composed at the `Application` layer via two separate repository/query calls, never a `.Include()` or join spanning `DbContext`s. This was already implicitly true under a single shared context too, but is now structurally enforced since there's no shared model to lean on by accident.

`ApplySoftDeleteFilter()` (and a future `ApplyTenantFilter()`, Part V) is called independently inside each module's own `OnModelCreating`, exactly as `UBP.Tenant.Persistence.Contexts.AppDbContext` calls it today — no change needed there.

This keeps every module's `Bootstrap.AddPersistence` exactly as self-contained as `UBP.Tenant`'s is right now (`AddDbContextPool<TheirOwnContext>` + `AddUnitOfWork`): no module needs to expose its `IEntityTypeConfiguration<T>` assembly to a host project, and the host (`UBP.Booking.API`) needs no `Persistence`-level reference to every module it hosts — only an `Application`-level one, to call each module's own `AddApplication`/`AddPersistence`.

---

## Part III. Closing the Entity Key-Type Gap

`UBP.Core.Domain.Entities.Entity` is now generic over its key, not fixed to `int`:

```csharp
// UBP.Core.Domain
public class Entity<TKey>
{
    public TKey Id { get; set; }
}
```

`EntityConfiguration<TEntity, TKey>`, `IEntityRepository<TEntity, TKey>`, and `EntityRepository<TEntity, TKey>` are generic the same way (`Delete(TKey id)` replacing `Delete(int id)`), and `RepositoryFactory` resolves `TKey` per entity by walking the base-type chain for the closed `Entity<TKey>` it derives from — so no entity needs to be registered by hand.

There is **no backward-compatible non-generic `Entity` anymore** — every entity must close `Entity<TKey>` with an explicit type argument, `int` or `Guid`, whichever the table's PK actually is. Use `Guid` only where the schema calls for it:

- **Booking module entities** (`AvailabilitySlotEntity`, `BookingEntity`, `BookingStatusHistoryEntity`, and the rest of `db-spec-mvp.md`'s `uuid`-PK tables) derive from `Entity<Guid>`, with PKs generated via `Guid.CreateVersion7()` in the entity constructor or a factory (`UBP.Core.Data.EF/Factories/RepositoryFactory.cs` or a new `IdFactory` are the natural homes — check before adding a third place PKs get generated).
- **`TenantEntity` declares its key type explicitly** — `TenantEntity : Entity<Guid>` (matching the app-side `Guid.CreateVersion7()` PK convention), per `db-spec-mvp.md`'s `tenants.id uuid` column. Fixed in `c9381b2`.

---

## Part IV. Domain Layer — Aggregates

Group the fifteen tables from `db-spec-mvp.md` into aggregates instead of one-entity-per-table classes with no ownership boundary:

| Aggregate root | Members | Owning module |
|---|---|---|
| `Resource` | `WeeklySchedule[]`, `ScheduleException[]`, `ResourceAsset[]` (link only — `Asset` itself lives in `UBP.Storage`) | `UBP.Booking` |
| `FieldSchema` | `FieldDefinition[]` — one shape, two `kind`s (`ResourceAttributes`, `BookingForm`), per Chapter 1 | `UBP.Booking` |
| — (no aggregate root; owned by `Resource`) | `ResourceAttributeValue[]` — current values for `kind=ResourceAttributes`, edited in place | `UBP.Booking` |
| `Booking` | `BookingFieldValue[]`, `BookingStatusHistory[]` | `UBP.Booking` |
| `TenantNotificationChannel` | — | `UBP.Notifications` |
| `Asset` | — (storage metadata only, no owner FK) | `UBP.Storage` |

Each aggregate gets exactly one repository (`IRepository<Resource>`, `IRepository<Booking>`, `IRepository<FieldSchema>`) — never a repository per child table. Children are loaded/saved through the root, consistent with how `Booking` in the DB spec already denormalizes `customer_*` fields onto itself specifically to avoid joining into `BookingFieldValue` for hot reads.

The **field-schema service** described in `db-spec-mvp.md` ("one service, one table pair") is a single `Application`-layer service — `FieldSchemaService` — parameterized by `kind`, shared by both the resource-attribute flow and the booking-form flow. Do not write two near-identical command sets for the two `kind`s; this is called out explicitly in both the platform spec and the DB spec as a rule, not a suggestion.

---

## Part V. Cross-Module Integration Points

### Tenant isolation (Appendix rule 8)

Rather than trusting every handler to remember `.Where(x => x.TenantId == tenantId)`, add an EF **global query filter** driven by an ambient `ITenantContext` (resolved per-request from the JWT `tenant_id` claim or route), the same mechanism already used for soft delete (`ModelBuilderExtensions.ApplySoftDeleteFilter`, `UBP.Core.Data.EF/Extensions/ModelBuilderExtensions.cs:11`):

```csharp
public static void ApplyTenantFilter(this ModelBuilder modelBuilder, Func<Guid> currentTenantId)
{
    foreach (var clrType in modelBuilder.Model.GetEntityTypes()
                 .Where(x => typeof(ITenantScoped).IsAssignableFrom(x.ClrType))
                 .Select(x => x.ClrType))
    {
        modelBuilder.Entity(clrType).HasQueryFilter(BuildTenantFilter(clrType, currentTenantId));
    }
}
```

`Resource`, `Booking`, `TenantNotificationChannel`, `Asset` all implement a new `ITenantScoped { Guid TenantId }` marker interface (parallel to `ISoftDeletable`/`IAuditable`) so the filter is opt-in per entity and impossible to forget once applied. This turns Appendix rule 8 from a code-review discipline into a schema-level guarantee, matching the project's existing pattern for cross-cutting invariants (soft delete, auditing).

### Notification dispatch (Booking → Notifications, without a hard reference)

`Booking.Application` command handlers (e.g. `CreateBookingCommandHandler`, `ConfirmBookingCommandHandler`) publish a small integration event after a successful `SaveChangesAsync`:

```csharp
public interface IBookingEventPublisher
{
    Task PublishAsync(BookingCreatedEvent evt, CancellationToken ct = default);
    // ...BookingConfirmedEvent, BookingCancelledEvent, BookingRescheduledEvent
}
```

`UBP.Notifications.Application` implements `IBookingEventPublisher`, looks up active `tenant_notification_channels` for that `tenant_id` + event type (the `jsonb @>` query already documented in `db-spec-mvp.md`), and fans out through per-channel-type senders (`IEmailSender`, `IWebhookSender`, `ISlackSender`) — each swappable, same "policy-enforcing interface, swappable provider" shape as `UBP.Cache.Abstractions`/`UBP.Cache.InMemory` already establishes in this repo for a different cross-cutting concern.

`Booking.Application` depends only on `IBookingEventPublisher` (an abstraction it or a shared contracts project defines) — never on `UBP.Notifications.Persistence` or its entities. `Notifications` depends on nothing from `Booking` beyond the event DTOs.

### Asset storage (Resource → Storage)

Exactly as specified in `docs/asset-storage-spec.md`: `Resource`'s asset-upload command depends on `IAssetStorage` (`Upload`/`GetAccessUrl`/`Delete`) from `UBP.Storage.Abstractions`, never on `UBP.Storage.Local`/`UBP.Storage.S3` concretely — provider selection is DI configuration, matching the `UBP.Cache.Abstractions` → `UBP.Cache.InMemory` precedent already in the solution.

---

## Part VI. Application Layer — Command/Query Catalog

Using `UBP.CQRS` (`IRequest<T>` / `IRequestHandler<TRequest, Result<T>>` / `ISender`) and `UBP.Result`, the same shape as `RegisterUserCommand` in `UBP.IAM.Application`:

| Phase (booking-platform-spec.md Ch.10) | Commands | Queries |
|---|---|---|
| 1 — Tenant & Resource | `CreateResourceCommand`, `UpdateResourceAttributesCommand`, `UploadResourceAssetCommand`, `DeleteResourceAssetCommand`, `CreateNotificationChannelCommand`, `ToggleNotificationChannelCommand` | `GetResourceQuery`, `ListNotificationChannelsQuery` |
| 2 — Form Builder | `CreateFieldDefinitionCommand`, `ReorderFieldDefinitionsCommand`, `PublishBookingFormSchemaCommand` (versioning, Ch.7) | `GetFieldSchemaQuery` (by `kind`), `PreviewBookingFormQuery` |
| 3 — Schedule & Availability | `SetWeeklyScheduleCommand`, `AddScheduleExceptionCommand` | `GetAvailableSlotsQuery` (date range → free slots, accounts for bookings + exceptions) |
| 4 — Booking Flow | `CreateBookingCommand` (Layers 1–3, Chapter 4) | `GetBookingByReferenceQuery` |
| 5 — Tenant Dashboard | `ConfirmBookingCommand`, `CancelBookingCommand` | `ListBookingsQuery` (filters: date, status, customer search), `GetBookingDetailQuery` |

Every command handler returns `Result` / `Result<T>` (never throws for expected business failures), matching `RegisterUserCommandHandler`'s existing pattern of mapping known failure modes to a typed `Error` (`UserErrors.AlreadyExists` there → `BookingErrors.SlotUnavailable`, `FieldSchemaErrors.BreakingChange`, etc. here).

---

## Part VII. Validation Pipeline (Chapter 4)

Implement the three layers as three distinct, composable pieces inside `Booking.Application`, not as one monolithic handler:

- **Layer 1 (per-field):** `IFieldValidator`, one implementation per `field_type`, reading `validation_rules` jsonb off `FieldDefinition`. Dispatched by `field_type`, not by `if/else` chains in the handler.
- **Layer 2 (cross-field):** a small rule evaluator over declared rules (e.g. `check_out_after_check_in`) — data, not arbitrary code, per Appendix rule 3 ("tenants never write code or arbitrary regex").
- **Layer 3 (business):** lives directly in `CreateBookingCommandHandler` — slot availability, min lead time, max horizon. This layer needs the DB, so it isn't a reusable pure-function validator like Layers 1–2.

All three layers run **server-side unconditionally**; the client repeats Layers 1–2 only for UX (Appendix rule 1). Do not let the client be the only enforcement point for anything, including conditional visibility (Chapter 5) — a hidden-but-still-submitted field must be rejected/ignored server-side too.

### Double-booking (Appendix rule 6)

The partial unique index in `db-spec-mvp.md` is the actual guarantee, not the application check. `CreateBookingCommandHandler` should still check availability before insert (for a fast, friendly error), but must also catch the `DbUpdateException`/`PostgresException` (unique violation on `(resource_id, slot_start_at)`) from the final `SaveChangesAsync` and map it to `Result.Failure(BookingErrors.SlotUnavailable)` — the pre-check has a race window between two concurrent requests that only the DB constraint closes.

---

## Part VIII. API Layer

`UBP.Booking.API` (the shared host process, not a shared `DbContext` — Part II) hosts, per bounded context, controllers mirroring `UBP.IAM.API`'s `Controllers/` pattern:

| Controller | Auth | Notes |
|---|---|---|
| `TenantsController` | JWT (owner) | registration is anonymous; profile is authenticated |
| `ResourcesController`, `FieldSchemasController`, `SchedulesController` | JWT (owner) | tenant-scoped via `ITenantScoped` filter (Part V) |
| `PublicBookingController` | anonymous | keyed by `{tenantSlug}/{resourceSlug}`, guest bookings (Chapter 9) |
| `BookingsController` | JWT (owner) | dashboard: list/detail/confirm/cancel |
| `NotificationChannelsController` | JWT (owner) | lives in `UBP.Notifications.API` or is exposed from the same host, TBD by whether Notifications gets its own API project or stays a library consumed by `Booking.API` |

JWT validation reuses IAM's issued tokens exactly as `UBP.IAM.API`'s own `AddValidation(options => options.UseLocalServer())` does — `Booking.API` needs the equivalent `AddValidation` pointed at IAM as the token-issuing authority (`UseAspNetCore()` + remote/local server validation), since it's a separate process from the auth server.

---

## Part IX. Open Decisions

1. ~~**Tenant's `int Id` vs. spec'd `uuid`** (Part III) — needs an explicit call, not a silent Booking-side workaround.~~ Decided and applied: `TenantEntity : Entity<Guid>`.
2. **Per-module migration history table naming** (Part II) — pick the convention (`__EFMigrationsHistory_<Module>`) before the second module's first migration is generated, not after a collision is hit in CI/deploy.
3. **Does `UBP.Notifications` get its own `API` project**, or is it a library only consumed by `Booking.API`'s controllers? Affects the `NotificationChannelsController` row above.
4. **Guid PK generation location** — one factory, not one ad hoc `Guid.CreateVersion7()` call per entity constructor, so the "for better index clustering" convention in `db-spec-mvp.md` is actually honored everywhere.
5. **`Storage` as a separate top-level module vs. folded into `Booking`** — kept separate here to mirror `UBP.Cache.Abstractions`/`UBP.Cache.InMemory` and because `asset-storage-spec.md` explicitly designs it to be reusable by non-Resource owners later.
