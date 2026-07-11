# DB Specification for MVP (Postgres + EF Core)

## General Conventions

### Postgres
- All timestamps — `timestamp with time zone` (`timestamptz`), stored in UTC.
- All PKs — `uuid` (for EF: `Guid`). Generated on the application side (`Guid.NewGuid()` or `Guid.CreateVersion7()` for better index clustering).
- Table and column names — `snake_case` (via `UseSnakeCaseNamingConvention()` from EFCore.NamingConventions).
- Enums stored as `varchar` (via `HasConversion<string>()`), so values are human-readable in the DB and don't break when new values are added.
- JSON — `jsonb`, not `json`.

### EF Core
- Version: EF Core 10+.
- Provider: `Npgsql.EntityFrameworkCore.PostgreSQL`.
- Naming conventions: `EFCore.NamingConventions` (`UseSnakeCaseNamingConvention()`).
- All configuration via **Fluent API** in `IEntityTypeConfiguration<T>`. No attributes on entities.
- One `DbContext` **per module** (`TenantDbContext`, `BookingDbContext`, `NotificationsDbContext`, ...), all pointed at the same domain database — not one shared `DbContext`/EF model spanning every module. Separate `DbContext` (and separate database) for the auth server (OpenIddict). See `docs/booking-engine-architecture-spec.md`, Part II, for the full rationale and the consequences (per-module migrations history table, hand-added cross-module FKs, no cross-context joins).

### Migrations
- Created with `dotnet ef migrations add InitialCreate`.
- Applied automatically on API startup in Development only. In Production — as a separate deployment step (`dotnet ef migrations bundle` or explicit `dotnet ef database update`).
- Migration names — descriptive: `AddBookingFieldValueIndexes`, `RenameResourceTimezone`, etc.
- Each module's `DbContext` sets its own `MigrationsHistoryTable` (e.g. `__EFMigrationsHistory_Booking`) so migrations from different modules against the same database don't collide on the default `__EFMigrationsHistory` table.

---

## Table: tenants

Purpose: business owner, root entity of multi-tenancy.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| name | varchar(200) | no | — | |
| slug | varchar(100) | no | — | UK, latin chars + hyphens |
| timezone | varchar(50) | no | — | IANA TZ, e.g. `Europe/Warsaw` |
| status | varchar(20) | no | `'Active'` | enum: `Active`, `Suspended` |
| created_at | timestamptz | no | `now()` | |
| updated_at | timestamptz | no | `now()` | updated in code |

**Indexes:**
- UK on `slug`
- index on `status`

**EF Core config:**
```csharp
builder.HasKey(t => t.Id);
builder.Property(t => t.Slug).HasMaxLength(100).IsRequired();
builder.HasIndex(t => t.Slug).IsUnique();
builder.Property(t => t.Status)
    .HasConversion<string>()
    .HasMaxLength(20);
```

---

## Table: tenant_users

Purpose: tenant administrator. Linked to the auth server via `external_auth_id`.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| tenant_id | uuid | no | — | FK → tenants |
| external_auth_id | varchar(255) | no | — | sub from JWT auth server |
| email | varchar(255) | no | — | |
| role | varchar(20) | no | `'Owner'` | enum: `Owner` only in MVP |
| created_at | timestamptz | no | `now()` | |

**Indexes:**
- index on `tenant_id`
- UK on `external_auth_id`
- UK on `(tenant_id, email)`

> **Note:** one person = one `external_auth_id` in the auth server. But they can own multiple tenants — so one `tenant_user` record per (tenant, user) pair.

---

## Table: tenant_notification_channels

Purpose: defines where system-generated notifications are sent for a tenant. Decouples delivery targets from user accounts entirely — the target can be a shared mailbox, a webhook, or a Slack hook, with no `tenant_user` account required.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| tenant_id | uuid | no | — | FK → tenants |
| channel_type | varchar(20) | no | — | enum: `Email`, `Webhook`, `Slack` |
| target | varchar(500) | no | — | email address, webhook URL, or Slack hook URL |
| event_types | jsonb | no | `'[]'` | e.g. `["BookingCreated", "BookingCancelled"]` |
| label | varchar(100) | yes | — | human-readable name, e.g. "Reception inbox" |
| is_active | boolean | no | `true` | |
| created_at | timestamptz | no | `now()` | |

**Indexes:**
- index on `tenant_id`
- index on `(tenant_id, channel_type)`

**Event types (initial set):**
`BookingCreated`, `BookingConfirmed`, `BookingCancelled`, `BookingRescheduled`, `DailySummary`

**Querying active channels for an event:**
```sql
SELECT * FROM tenant_notification_channels
WHERE tenant_id = $1
  AND is_active = true
  AND event_types @> '["BookingCreated"]'::jsonb;
```

> **Seeding on registration:** when a tenant is created, automatically insert one default `Email` channel pointed at the owner's email with all event types enabled, so notifications work out of the box without extra configuration.

---

## Table: resources

Purpose: the thing being booked. One per tenant in MVP, but the architecture supports many.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| tenant_id | uuid | no | — | FK |
| name | varchar(200) | no | — | |
| slug | varchar(100) | no | — | unique within tenant |
| description | text | yes | — | |
| timezone | varchar(50) | yes | — | if null — inherited from tenant |
| auto_confirm | boolean | no | `true` | |
| min_lead_time_minutes | int | no | `0` | |
| max_booking_days_ahead | int | no | `90` | |
| status | varchar(20) | no | `'Draft'` | enum: `Draft`, `Active`, `Archived` |
| created_at | timestamptz | no | `now()` | |
| updated_at | timestamptz | no | `now()` | |

**Indexes:**
- index on `tenant_id`
- UK on `(tenant_id, slug)`

---

## Table: assets

Purpose: owned entirely by the asset storage implementation (see `docs/asset-storage-spec.md`) — one row per stored object, independent of what references it. Metadata only — the actual bytes live in the storage backend.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| storage_key | varchar(500) | no | — | key/path in the storage backend, `{asset_id}.{ext}` |
| content_type | varchar(100) | no | — | MIME type, stored as blob metadata; not used for upload policy |
| size_bytes | bigint | no | — | validated against policy at upload time |
| created_at | timestamptz | no | `now()` | |

**Indexes:** none beyond the PK for now.

> **No `tenant_id` yet:** deferred for now, not dropped — tenant scoping for stored objects (denormalized `tenant_id` + storage-key prefixing) is expected to come back once tenant isolation (Appendix rule 8) is wired up across the domain; see `docs/asset-storage-spec.md`.

> **Why no `url` column:** the public/signed URL is derived from `storage_key` by the asset storage layer at read time, not stored — this keeps the row valid if the storage backend or CDN domain changes.
> **Row lifecycle:** a row is inserted only after the object is confirmed written to storage, and deleted only after the object is confirmed removed — never leave a row pointing at a missing object.
> **No `resource_id` here:** this table doesn't know what it's attached to — that's the job of a link table like `resource_assets` below. Keeps the storage implementation reusable by any future owner (tenant logo, booking attachment, ...) without a schema change.

---

## Table: resource_assets

Purpose: links a resource to its assets and orders them. Owned by the resource domain, not the storage implementation — knows nothing about `storage_key`, `content_type`, etc.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| resource_id | uuid | no | — | FK → resources |
| asset_id | uuid | no | — | FK → assets |
| display_order | int | no | `0` | for sorting |

**Indexes:**
- index on `resource_id`
- UK on `(resource_id, asset_id)`

---

## Table: weekly_schedules

Purpose: working hours by day of week. One resource → one record per day of week.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| id | uuid | no | PK |
| resource_id | uuid | no | FK |
| day_of_week | int | no | 0=Sun..6=Sat |
| start_time | time | no | |
| end_time | time | no | |
| slot_duration_minutes | int | no | e.g. 30, 60 |
| buffer_minutes | int | no | between slots |
| is_active | boolean | no | allows disabling a day |

**Indexes:**
- index on `resource_id`
- UK on `(resource_id, day_of_week)` — MVP does not allow multiple intervals per day.

> **EF Core config for `time`:** In .NET 6+ — `TimeOnly`. Supported natively by Npgsql.

---

## Table: schedule_exceptions

Purpose: date blocks (vacation, maintenance, holidays).

| Column | Type | Nullable | Notes |
|---|---|---|---|
| id | uuid | no | PK |
| resource_id | uuid | no | FK |
| date | date | no | `DateOnly` in C# |
| start_time | time | yes | if null — entire day |
| end_time | time | yes | |
| reason | varchar(200) | yes | |

**Indexes:** index on `(resource_id, date)`.

---

## Table: field_schemas

Purpose: generic container for a set of tenant-defined fields. Shared by both configuration levels from Chapter 1 (`docs/booking-platform-spec.md`) instead of duplicating near-identical tables per use case — the resource's own descriptive attributes and the booking-time client form differ only in `kind`, not in shape. One schema of each `kind` per resource in MVP.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| resource_id | uuid | no | — | FK |
| kind | varchar(20) | no | — | enum: `ResourceAttributes`, `BookingForm` |
| version | int | yes | `1` | meaningful only for `kind = BookingForm` (Chapter 7, Schema Versioning); unused/ignored for `ResourceAttributes`, which is always edited in place |
| created_at | timestamptz | no | `now()` | |
| updated_at | timestamptz | no | `now()` | |

**Indexes:** UK on `(resource_id, kind)`.

---

## Table: field_definitions

Purpose: individual field definitions within a `field_schemas` row — the single shared shape for both configuration levels (Chapters 2–6 of `docs/booking-platform-spec.md`).

| Column | Type | Nullable | Notes |
|---|---|---|---|
| id | uuid | no | PK |
| schema_id | uuid | no | FK → `field_schemas` |
| field_key | varchar(50) | no | stable machine name |
| label | varchar(200) | no | |
| field_type | varchar(20) | no | `ShortText`, `Number`, `Date`, `SingleSelect`, `Checkbox` |
| is_required | boolean | no | |
| is_system | boolean | no, default `false` | meaningful only when the parent schema's `kind = BookingForm` (system fields cannot be deleted); always `false` for `ResourceAttributes` — the engine mandates no default resource attributes |
| display_order | int | no | |
| placeholder | varchar(200) | yes | |
| help_text | varchar(500) | yes | |
| validation_rules | jsonb | yes | `{ "minLength": 1, "maxLength": 100, "min": 0, "max": 10 }` |
| options | jsonb | yes | for selects: `[{"value": "a", "label": "Option A"}]` |

**Indexes:**
- index on `schema_id`
- UK on `(schema_id, field_key)`

**EF Core config for jsonb:**
```csharp
builder.Property(f => f.ValidationRules)
    .HasColumnType("jsonb");

// If you want a typed object:
builder.Property(f => f.ValidationRules)
    .HasColumnType("jsonb")
    .HasConversion(
        v => JsonSerializer.Serialize(v, JsonOptions),
        v => JsonSerializer.Deserialize<ValidationRules>(v, JsonOptions));
```

**System fields** (created automatically on the `BookingForm` schema when a resource is created):

| field_key | Type | Required | Notes |
|---|---|---|---|
| customer_name | ShortText | yes | is_system = true |
| customer_email | ShortText | yes | with email validation |
| customer_phone | ShortText | yes | |
| customer_notes | ShortText | no | |

> Slot selection is a separate entity (`slot_start_at` in booking), not a booking field.

> **One service, one table pair:** CRUD, Layer 1/2 validation (Chapter 4), and conditional-logic evaluation (Chapter 5) for `field_schemas`/`field_definitions` are implemented once, in a single field-schema service parameterized by `kind` — neither the resource-attribute flow nor the booking-form flow gets its own copy of this logic.

---

## Table: resource_attribute_values

Purpose: the owner-filled current value for each field of the resource's `ResourceAttributes` schema. Always current — overwritten in place when the owner edits, never versioned or snapshotted.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| id | uuid | no | PK |
| resource_id | uuid | no | FK, denormalized for direct queries |
| field_id | uuid | no | FK → `field_definitions`, `CASCADE` |
| value | jsonb | no | universal storage — same format convention as `booking_field_values` |
| updated_at | timestamptz | no | |

**Indexes:**
- index on `resource_id`
- UK on `(resource_id, field_id)`

> **Why FK to `field_id` instead of duplicating `field_key`/`field_type`** (unlike `booking_field_values`, which duplicates them for resilience): these values are not a historical snapshot — they must always reflect the current schema. If a field is deleted, `CASCADE` removes its values too; there's no snapshot to protect.

---

## Table: bookings

Purpose: the core entity of the system.

| Column | Type | Nullable | Default | Notes |
|---|---|---|---|---|
| id | uuid | no | — | PK |
| tenant_id | uuid | no | — | denormalized for filtering |
| resource_id | uuid | no | — | FK |
| schema_id | uuid | no | — | FK → `field_schemas` (`kind = BookingForm`), which schema version was used |
| slot_start_at | timestamptz | no | — | UTC |
| slot_end_at | timestamptz | no | — | UTC |
| status | varchar(20) | no | `'Pending'` | enum |
| customer_name | varchar(200) | no | — | duplicated from system fields for query convenience |
| customer_email | varchar(255) | no | — | |
| customer_phone | varchar(50) | no | — | |
| customer_notes | text | yes | — | |
| external_reference | varchar(20) | no | — | UK, human-readable code e.g. `BK-2026-00042` |
| created_at | timestamptz | no | `now()` | |
| updated_at | timestamptz | no | `now()` | |
| confirmed_at | timestamptz | yes | — | |
| cancelled_at | timestamptz | yes | — | |
| cancellation_reason | varchar(500) | yes | — | |

**Indexes (critical):**
- index on `tenant_id`
- index on `resource_id`
- index on `(resource_id, slot_start_at)`
- **partial UK** on `(resource_id, slot_start_at) WHERE status IN ('Pending', 'Confirmed')` — prevents double booking
- UK on `external_reference`
- index on `status`
- index on `customer_email` (for search)

**EF Core config for partial unique index:**
```csharp
builder.HasIndex(b => new { b.ResourceId, b.SlotStartAt })
    .HasFilter("status IN ('Pending', 'Confirmed')")
    .IsUnique();
```

> **Why denormalize `customer_*` in booking when the data exists in `booking_field_values`:**
> - Searching by client name/email is a frequent operation in the tenant dashboard.
> - Email is needed for sending notifications without a join.
> - This is a deliberate trade-off against frequent joins on hot queries.

---

## Table: booking_field_values

Purpose: snapshot of all form field values at the time the booking was created.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| id | uuid | no | PK |
| booking_id | uuid | no | FK |
| field_key | varchar(50) | no | duplicated from field_definitions for resilience |
| field_type | varchar(20) | no | duplicated for correct rendering |
| value | jsonb | no | universal storage |

**Indexes:**
- index on `booking_id`
- UK on `(booking_id, field_key)`

**`value` format (jsonb):**

| field_type | Example value |
|---|---|
| ShortText | `"string"` |
| Number | `42` or `42.5` |
| Date | `"2026-01-15"` |
| SingleSelect | `"value_a"` |
| Checkbox | `true` or `false` |

The universal format allows storing any type, and `field_type` tells the frontend how to render it.

---

## Table: booking_status_history

Purpose: audit log of booking status changes.

| Column | Type | Nullable | Notes |
|---|---|---|---|
| id | uuid | no | PK |
| booking_id | uuid | no | FK |
| from_status | varchar(20) | yes | null when booking is created |
| to_status | varchar(20) | no | |
| changed_by_user_id | uuid | yes | null if action by customer or system |
| changed_by_type | varchar(20) | no | `Customer`, `TenantUser`, `System` |
| reason | varchar(500) | yes | |
| changed_at | timestamptz | no | |

**Indexes:**
- index on `booking_id`
- index on `changed_at`

---

## Additional DB-Level Rules

### Cascading Deletes

| Parent | Child | Behavior | Reason |
|---|---|---|---|
| `Tenant` | `TenantUser`, `Resource`, `TenantNotificationChannel` | `RESTRICT` / `CASCADE` | Users and resources: `RESTRICT`. Notification channels: `CASCADE` — they're config, not business data. |
| `Resource` | `ResourceAsset` (link), `WeeklySchedule`, `ScheduleException` | `CASCADE` | Deleting a resource removes its asset links; `Asset` rows are not FK-linked to `Resource` and are not cascaded (see Asset Storage) |
| `Resource` | `Booking` | `RESTRICT` | Cannot delete a resource with active bookings — archive first |
| `Resource` | `FieldSchema` (both `kind`s) → `FieldDefinition` | `CASCADE` | |
| `Resource` | `ResourceAttributeValue` (direct FK) | `CASCADE` | Also removed directly since `resource_id` is denormalized on the value row (in addition to the cascade arriving via `FieldDefinition`) |
| `Booking` | `BookingFieldValue`, `BookingStatusHistory` | `CASCADE` | |

### Soft Delete
Not introduced in MVP. `status='Archived'` or `status='Suspended'` is sufficient. If needed — add a `deleted_at` column in a separate migration.

### `updated_at` Trigger
Can be done via a Postgres trigger or updated in `SaveChangesAsync()` by overriding `DbContext`. Recommendation — the latter, keeps it closer to the code.

```csharp
public override Task<int> SaveChangesAsync(CancellationToken ct = default)
{
    var now = DateTime.UtcNow;
    foreach (var entry in ChangeTracker.Entries<IHasTimestamps>())
    {
        if (entry.State == EntityState.Added) entry.Entity.CreatedAt = now;
        if (entry.State is EntityState.Added or EntityState.Modified)
            entry.Entity.UpdatedAt = now;
    }
    return base.SaveChangesAsync(ct);
}
```

### Timezones
- In DB — UTC only via `timestamptz`.
- API serialization — ISO 8601 with `Z`.
- In UI — conversion to tenant/client timezone on the frontend.
- Never store "local time" as `timestamp without time zone`.

---

## Phase 0 Checklist After Schema Creation

- [ ] Install EF Core 10+ and `Npgsql.EntityFrameworkCore.PostgreSQL`.
- [ ] Install `EFCore.NamingConventions`.
- [ ] Create each module's own `DbContext` (e.g. `BookingDbContext`) with `IEntityTypeConfiguration<T>` for that module's own entities only, plus its own `MigrationsHistoryTable`.
- [ ] Generate the first migration `InitialCreate`.
- [ ] Spin up Postgres locally via docker-compose.
- [ ] Apply the migration.
- [ ] Verify the DB schema matches expectations (`psql \d+ bookings`).
- [ ] Write first integration tests with Testcontainers for Postgres.