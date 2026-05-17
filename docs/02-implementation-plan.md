# Slot — Implementation Plan

## Releases

| Phase | Folder | Scope |
|-------|--------|-------|
| 1 | [phases/phase-1/](phases/phase-1/README.md) | Core booking engine, NoSQL-backed tenant config |
| 2 | — | Admin API for runtime configuration |
| 3 | — | Pre-built domain templates (fitness, barbershop, clinic) |

---

## Phase 1 — Steps

Complete steps in order — each layer depends on the one before it.

| # | Document | Layer | What |
|---|----------|-------|------|
| 1 | [01-domain.md](phases/phase-1/01-domain.md) | Domain | Entities, enums, domain methods, exceptions |
| 2 | [02-persistence.md](phases/phase-1/02-persistence.md) | Persistence | EF Core mapping, repositories, migrations |
| 3 | [03-config.md](phases/phase-1/03-config.md) | Config | MongoDB tenant and resource config, caching |
| 4 | [04-application.md](phases/phase-1/04-application.md) | Application | Use cases and business orchestration |
| 5 | [05-api.md](phases/phase-1/05-api.md) | API | HTTP endpoints, middleware, validation |

> Steps 2 and 3 are independent of each other and can be worked on in parallel.
