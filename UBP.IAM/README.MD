# UBP.IAM — Identity & Access Management

OAuth 2.0 / OpenID Connect server built on OpenIddict + ASP.NET Core Identity.

## Responsibilities

- Authenticates tenant users (business owners).
- Issues JWT access tokens and refresh tokens via OAuth 2.0 Authorization Code Flow with PKCE.
- Maintains its own user database; has no knowledge of tenants.

The user-to-tenant link is established in the main database via `tenant_users.external_auth_id` (the `sub` claim from the JWT).

---

## API Documentation

Available in the **Development** environment only at:

```
/scalar/v1
```

---

## Endpoints

| Path | Description |
|---|---|
| `/.well-known/openid-configuration` | OIDC discovery document |
| `/.well-known/jwks` | Public keys for JWT validation |
| `/connect/authorize` | Authorization Code Flow entry point |
| `/connect/token` | Code → access token / refresh token exchange |
| `/connect/userinfo` | User claims from access token |
| `/connect/logout` | Single sign-out |
| `POST /auth/register` | User registration |

---

## Database Migrations

### Prerequisites

Install the EF Core CLI tools (once per machine):

```powershell
dotnet tool install --global dotnet-ef
```

### Create a new migration

Run from the repository root:

```powershell
dotnet ef migrations add <MigrationName> `
  --project UBP.IAM/UBP.IAM.Persistence `
  --startup-project UBP.IAM/UBP.IAM.API `
  --output-dir Migrations
```

### Apply migrations

```powershell
dotnet ef database update `
  --project UBP.IAM/UBP.IAM.Persistence `
  --startup-project UBP.IAM/UBP.IAM.API
```

> Migrations are automatically applied on startup in the Development environment.

---

## Configuration

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "AuthDb": "Host=localhost;Port=5432;Database=bookingplatform_auth;Username=postgres;Password=postgres"
  }
}
```

---

## Production Checklist

1. **Signing certificates** — replace `AddDevelopmentEncryptionCertificate` / `AddDevelopmentSigningCertificate` with persistent certificates so tokens are not invalidated on restart.
2. **HTTPS** — required by OpenIddict.
3. **CORS** — configure allowed origins for the SPA domain.
4. **Rate limiting** — apply to `/connect/token` and `/connect/authorize`.
