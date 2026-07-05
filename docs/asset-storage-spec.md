# Asset Storage

Referenced from `docs/booking-platform-spec.md`, Part II (Tenant Configuration), and `docs/db-spec-mvp.md` (`assets`, `resource_assets` tables).

Resources carry file attachments — photos in MVP, with documents/videos reserved for later. Storage is split across two tables so the storage implementation and the resource domain stay decoupled:
- **`assets`** — owned entirely by the asset storage component. One row per stored object: `storage_key`, `content_type`, `size_bytes`, `asset_type`. Knows nothing about resources.
- **`resource_assets`** — owned by the resource domain. A pure link table (`resource_id`, `asset_id`, `display_order`) saying which assets belong to which resource, in what order.

**Why separate:** the storage backend (local disk in dev, an S3-compatible bucket or Blob store in prod) is an infrastructure detail that must be swappable without touching booking/resource domain logic, and every write to it must pass through the same policy checks regardless of which tenant or resource is involved. Keeping `assets` free of a `resource_id` also means the same component can back other owners later (a tenant logo, a booking attachment, ...) without a schema change — the domain only ever adds a new link table, never touches `assets`.

**Abstraction:**
- A single interface (e.g. `IAssetStorage`) exposes `Upload`, `GetAccessUrl`, `Delete` — domain code never touches file paths, buckets, or SDKs directly.
- The concrete provider (local filesystem, S3-compatible, Azure Blob, ...) is selected by configuration, not by code changes.

**Storage key convention:**
- `{tenant_id}/{asset_id}.{ext}` — tenant isolation is enforced at the storage layer itself, not only in DB queries (extends Appendix rule 8 in `docs/booking-platform-spec.md` to stored objects). `tenant_id` is denormalized onto `assets` for this reason even though the table is otherwise owner-agnostic.

**Required policies (enforced on upload, before an `assets` row is written):**
- **Allowed content types** — a whitelist per `asset_type` (e.g. `image/jpeg`, `image/png`, `image/webp` for `Photo`). No arbitrary MIME types.
- **Max file size** — a fixed cap per `asset_type` (MVP: photos only, single cap).
- **Max asset count per resource** — MVP: 1–3 photos per resource (enforced against `resource_assets`, not `assets`).
- **Access mode** — MVP: public read via a stable URL. Private/signed URLs are a post-MVP policy option, not a default.

**Lifecycle rules:**
- An `assets` row is inserted only *after* the object is confirmed written to storage — never before.
- Deleting a resource cascades its `resource_assets` link rows (per `docs/db-spec-mvp.md`), not the `assets` rows themselves — `assets` has no FK to `resource`. Any `assets` row left with zero remaining links, plus its underlying storage object, is removed by a periodic reconciliation job (link deletion and storage deletion aren't atomic, so this also catches partial failures).

**Post-MVP:** private/signed access, virus scanning on upload, per-tenant storage quotas, additional `asset_type`s (documents, video), reuse of `assets` by owners other than `Resource`.
