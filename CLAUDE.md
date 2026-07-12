# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Universal Booking Platform (UBP) — an abstract booking engine. Core mechanics (tenants, resources, time, booking
statuses) are shared by every vertical (house rentals, fitness classes, appointments); anything domain-specific is
tenant *configuration*, never a fork. Full business rules and DB schema live in `docs/`:

- `docs/booking-platform-spec.md` — business requirements, MVP phases, and the **Appendix "Core Rules That Must
  Never Be Violated"** — read this before touching booking/resource/schema logic. Rules include: server is the
  single source of truth, bookings snapshot values at creation time and are never recalculated, tenants configure
  via data/rules only (never code/regex), UTC storage everywhere, stable field identifiers, DB-enforced
  double-booking prevention, notification targets decoupled from user accounts, asset storage abstracted behind a
  policy-enforcing interface.
- `docs/booking-engine-architecture-spec.md` — module topology, per-module `DbContext` design, dependency
  directions, cross-module integration points (notifications, asset storage).
- `docs/db-spec-mvp.md` — full Postgres schema, EF Core conventions, indexes, cascade rules.
- `docs/asset-storage-spec.md` — the `assets`/`resource_assets` split and upload policy rules.

These docs describe both current and planned state — check the actual code (below) before assuming something
described there is implemented.

## Solution layout

Flat `UBP.*` projects at repo root (see `UBP.slnx`), grouped by bounded context. Each business module
(`UBP.IAM`, `UBP.Tenant`, `UBP.Booking`, `UBP.Storage`) follows the same 4-layer split:

```
UBP.<Module>/
  UBP.<Module>.Domain/        # entities, enums — no EF, no framework deps
  UBP.<Module>.Persistence/   # internal sealed AppDbContext, IEntityTypeConfiguration<T>, repositories
  UBP.<Module>.Application/   # CQRS commands/queries + handlers, validation, *Errors static classes
  UBP.<Module>.API/           # Program.cs host, Endpoints/ (minimal API) or Controllers/
```

Each module owns its **own `DbContext`**, all pointed at the same Postgres database except IAM (separate auth DB) —
there is no shared/composed EF model across modules. Cross-module data references are bare denormalized IDs
(e.g. `Booking.TenantId : Guid`), never a project reference to another module's `Domain`/`Persistence`. Cross-module
behavior (e.g. Booking → Notifications) goes through an `Application`-layer abstraction, not a direct dependency.

Shared cross-cutting libraries (no business logic):

| Project | Purpose |
|---|---|
| `UBP.CQRS` | `IRequest`/`IRequestHandler`/`ISender` + `Sender` (reflection-based DI dispatch — **no pipeline/behaviors**, unlike MediatR) |
| `UBP.Result` | `Result`/`Result<T>`/`Error` — handlers return `Result`, never throw for expected business failures |
| `UBP.Core.Domain` | `Entity<TKey>` base, `ISoftDeletable`, `IAuditable`, `IActivatable` marker interfaces |
| `UBP.Core.Persistence` / `UBP.Core.Persistence.EF` / `UBP.Core.Data.EF` | repository/unit-of-work abstractions → EF adapter → entity-aware layer (soft-delete filter, `SaveChangesInterceptor`s for audit timestamps and soft delete, `RepositoryFactory`, `EntityConfiguration<TEntity,TKey>`) |
| `UBP.Endpoints` | `IEndpoint` + `AddEndpoints()`/`MapEndpoints()` minimal-API scanning convention |
| `UBP.Auth` | `AddIamAuthentication` — OpenIddict token *validation* against the IAM authority, for resource APIs |
| `UBP.Cache.Abstractions` / `UBP.Cache.InMemory` | `ICache` abstraction + `IMemoryCache`-backed implementation |
| `UBP.Logging` | Serilog wiring (console + rolling JSON file) |
| `UBP.Options.Configuration` | generic `IConfigureOptions<T>` binder |
| `UBP.OpenApi` / `UBP.OpenApi.Scalar` | OpenAPI doc generation + Scalar UI (Development only) |

## Conventions to follow

- **CQRS via `UBP.CQRS`, not MediatR.** Commands/queries are `record`s implementing `IRequest<Result>` /
  `IRequest<Result<T>>`; handlers are `internal sealed class ...CommandHandler : IRequestHandler<TRequest, Result<T>>`
  with a single `HandleAsync`. Register per module with `services.AddMessaging(Assembly.GetExecutingAssembly())`
  inside that module's `Application.Bootstrap.AddApplication`.
- **Errors are static fields, not exceptions.** Each `Application` project has an `Errors/` folder with static
  classes like `UserErrors`, `BookingErrors`, `SlotErrors` exposing `public static readonly Error X` (plus factory
  methods for parameterized errors). Map known failure modes to these; only let genuinely unexpected failures throw.
- **Entities close `Entity<TKey>` explicitly** — always `Entity<Guid>` or `Entity<int>` matching the table's actual
  PK type, never bare `Entity`. New tables per `db-spec-mvp.md` use `uuid` PKs generated app-side
  (`Guid.CreateVersion7()`) — follow the existing generation convention in a module before introducing a new one.
- **EF configuration is Fluent API only**, one `IEntityTypeConfiguration<T>` per entity in `Persistence/Configurations/`,
  extending `EntityConfiguration<TEntity, TKey>(tableName)`. No data-annotation attributes on entities.
- **`snake_case` DB naming** via `EFCore.NamingConventions`; timestamps are `timestamptz`/UTC always; enums stored
  as `varchar` via `HasConversion<string>()`; jsonb for flexible/schema-defined data (never `json`).
- **Soft delete and audit timestamps are interceptor-driven**, not `SaveChangesAsync` overrides — implement
  `ISoftDeletable`/`IAuditable` on an entity and the existing `SoftDeletableInterceptor`/`AuditableInterceptor`
  (`UBP.Core.Data.EF`) handle the rest automatically. Don't hand-roll this per module.
- **Minimal-API endpoints, in every module, get one file per resource group and one file per handler** — never a
  single class with all of a resource's routes inline. The route group itself is its own class implementing
  `IEndpointGroup` (`UBP.Endpoints.Interfaces`, exposing `string Prefix`), e.g. `AssetEndpointGroupV1`. Each
  HTTP verb/action against that group is its own class in its own file (e.g. `CreateAssetEndpoint`,
  `GetAssetEndpoint`, `DeleteAssetEndpoint`) implementing `IGroupEndpoint<TGroup>`, which maps a single
  `MapEndpoint(IEndpointRouteBuilder builder)` route, calls `ISender`, and maps `Result`/`Result<T>` to
  `Ok`/`BadRequest`/`NotFound`/`NoContent`. `Bootstrap.AddEndpoints`/`MapEndpoints` (`UBP.Endpoints`) discover
  groups and their handlers via reflection, so adding a file is enough — no manual registration. It's fine to add
  new interfaces/abstractions in `UBP.Endpoints` when a new endpoint shape doesn't fit `IEndpoint`/`IEndpointGroup`/
  `IGroupEndpoint<TGroup>` cleanly. `UBP.Booking.API/Endpoints` (`BookingEndpoints.cs`, `AvailabilityEndpoints.cs`)
  still uses the older one-class-per-resource-group style and hasn't been migrated — don't treat it as the
  reference when adding new endpoints. **IAM is the one exception** — it uses MVC Controllers + `AddRazorPages()`
  because of OpenIddict's UI flow requirements; don't "fix" it to match the endpoint pattern.
- **API versioning composes a group's prefix from another group rather than hardcoding it.** A per-module
  `V1Group : IEndpointGroup { Prefix => "/api/v1" }` owns the version segment; a resource group like
  `AssetEndpointGroupV1` builds its own `Prefix` off a `V1Group` instance (`$"{Parent.Prefix}/assets"`) instead of
  repeating the literal `"/api/v1"` string. No extra interface is needed for this — every group is still a plain
  `IEndpointGroup`, `Bootstrap.MapEndpoints` maps them all flat, and composition is just one group reading
  another's `Prefix`. To support v1 and v2 concurrently: add a `V2Group`, give the changed resource its own
  `...GroupV2` + version-specific handler classes, and have any handler that's identical across versions implement
  `IGroupEndpoint<TGroup>` for both group types on the same class — `Bootstrap` already registers one descriptor
  per closed `IGroupEndpoint<TGroup>` a type implements, so no extra wiring is needed.
- **Each module's migrations history table is named explicitly** (`__EFMigrationsHistory_<Module>`) since multiple
  `DbContext`s share one physical database — set this in that module's `Bootstrap.AddPersistence` when adding a
  new module's first migration.
- **Tenant isolation**: every tenant-scoped query must filter by `tenant_id`. There is currently no EF global query
  filter enforcing this automatically (`ITenantScoped`/`ApplyTenantFilter` described in the architecture doc do not
  exist in code yet) — until that lands, filtering is a manual per-handler discipline. Don't skip it.
- Style is enforced by `.editorconfig` + `Directory.Build.props` (`TreatWarningsAsErrors = true`,
  `EnforceCodeStyleInBuild = true`, SonarAnalyzer on every project): Allman braces, `using` directives outside the
  namespace, and **explicit types preferred over `var`** (`csharp_style_var_*` rules are all `false`).

## Commands

No `global.json`/`nuget.config` — relies on a locally installed .NET 10 SDK.

```powershell
# Build / restore everything
dotnet build UBP.slnx
dotnet restore UBP.slnx

# Format check (matches what CI intends to run)
dotnet format UBP.slnx --verify-no-changes --severity error

# Run a specific API locally (each has its own launchSettings.json / port)
dotnet run --project UBP.IAM/UBP.IAM.API          # https://localhost:7278
dotnet run --project UBP.Booking/UBP.Booking.API  # https://localhost:7279
dotnet run --project UBP.Storage/UBP.Storage.API  # https://localhost:7280

# Local Postgres per module (each module has its own docker-compose.yml, own port)
docker compose -f UBP.IAM/docker-compose.yml up -d
docker compose -f UBP.Booking/docker-compose.yml up -d      # Postgres on 5433, pgAdmin on 5051
docker compose -f UBP.Storage/docker-compose.yml up -d

# EF Core migrations (run from repo root; requires `dotnet tool install --global dotnet-ef` once)
dotnet ef migrations add <Name> --project UBP.<Module>/UBP.<Module>.Persistence --startup-project UBP.<Module>/UBP.<Module>.API --output-dir Migrations
dotnet ef database update --project UBP.<Module>/UBP.<Module>.Persistence --startup-project UBP.<Module>/UBP.<Module>.API
# Migrations auto-apply on startup in Development only; Production applies them as a separate deploy step.
```

There are **no test projects in the solution** (no `*.Tests.csproj`, no xUnit/NUnit/Testcontainers) — there is no
`dotnet test` to run. `.github/workflows/backend.yml` and `frontend.yml` reference paths that don't exist in this
repo (`src/`, `tests/`, `web/`, `BookingPlatform.Infrastructure`/`.Api`) — they're aspirational/stale, not a
description of the current build. Don't assume CI passes or that these paths are real without checking first.