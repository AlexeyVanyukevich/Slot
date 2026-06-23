# BookingPlatform.AuthServer

OAuth 2.0 / OpenID Connect сервер на OpenIddict + ASP.NET Core Identity.

## Назначение

- Аутентифицирует пользователей-тенантов (владельцев бизнесов).
- Выдаёт JWT access tokens и refresh tokens по OAuth 2.0 Authorization Code Flow с PKCE.
- Хранит пользователей в собственной БД, **не знает про тенантов**.

Связка «пользователь → тенант» делается в основной БД через `tenant_users.external_auth_id` (это `sub` из JWT).

---

## Структура проекта

```
BookingPlatform.IAM/
├── Program.cs
├── appsettings.json
├── appsettings.Development.json
├── Data/
│   ├── AuthDbContext.cs
│   └── ApplicationUser.cs
├── Controllers/
│   └── AuthorizationController.cs
├── Services/
│   └── OpenIddictSeeder.cs
└── Areas/Identity/Pages/         # default Identity UI (создастся автоматически)
```

---

## Установка

Создание проекта:

```bash
dotnet new webapp -n BookingPlatform.AuthServer -o src/BookingPlatform.AuthServer
cd src/BookingPlatform.AuthServer
```

Установка пакетов:

```bash
# Identity + EF Core
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add package Microsoft.AspNetCore.Identity.UI
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL

# Snake_case naming
dotnet add package EFCore.NamingConventions

# OpenIddict
dotnet add package OpenIddict.AspNetCore
dotnet add package OpenIddict.EntityFrameworkCore
```

---

## Конфигурация в `appsettings.json`

```json
{
  "ConnectionStrings": {
    "AuthDb": "Host=localhost;Port=5432;Database=bookingplatform_auth;Username=postgres;Password=postgres"
  },
  "OpenIddict": {
    "Clients": {
      "SpaWeb": {
        "ClientId": "spa-web",
        "DisplayName": "Booking Platform Web",
        "RedirectUris": [
          "http://localhost:5173/auth/callback"
        ],
        "PostLogoutRedirectUris": [
          "http://localhost:5173/auth/logout-callback"
        ]
      }
    }
  }
}
```

---

## Первая миграция

```bash
dotnet ef migrations add InitialCreate \
  --project src/BookingPlatform.AuthServer \
  --output-dir Data/Migrations

dotnet ef database update \
  --project src/BookingPlatform.AuthServer
```

При первом запуске `OpenIddictSeeder` зарегистрирует клиента `spa-web` и стандартные скоупы (`openid`, `profile`, `email`, `offline_access`, `api`).

---

## Endpoints, которые поднимутся

- `/.well-known/openid-configuration` — discovery.
- `/.well-known/jwks` — публичные ключи для валидации JWT.
- `/connect/authorize` — начало OAuth-флоу.
- `/connect/token` — обмен кода на токен / refresh token.
- `/connect/userinfo` — информация о пользователе по access token.
- `/connect/logout` — единый выход.
- `/Identity/Account/Login`, `/Identity/Account/Register` — UI логина/регистрации.

---

## Интеграция с основным API

В `BookingPlatform.Api` добавь:

```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = builder.Configuration["AuthServer:Authority"];
        options.Audience = "api";
        options.TokenValidationParameters.ValidateAudience = true;
    });
```

В `appsettings.json` основного API:
```json
{
  "AuthServer": {
    "Authority": "https://localhost:5001"
  }
}
```

Основной API при первом запросе подтянет JWKS с auth-сервера и будет валидировать токены **локально**, без сетевых вызовов на каждый запрос.

---

## Интеграция с React SPA

Установить библиотеку:
```bash
npm install oidc-client-ts react-oidc-context
```

```tsx
// src/auth/config.ts
import { UserManagerSettings } from 'oidc-client-ts';

export const oidcConfig: UserManagerSettings = {
  authority: 'https://localhost:5001',
  client_id: 'spa-web',
  redirect_uri: `${window.location.origin}/auth/callback`,
  post_logout_redirect_uri: `${window.location.origin}/auth/logout-callback`,
  response_type: 'code',
  scope: 'openid profile email offline_access api',
  loadUserInfo: true,
};
```

PKCE обрабатывается библиотекой автоматически.

---

## Что НЕ входит в эту минимальную конфигурацию (на потом)

- Подтверждение email при регистрации (поставлено `RequireConfirmedEmail = false`).
- 2FA.
- Восстановление пароля по email (UI есть, но SMTP не настроен).
- Внешние провайдеры (Google, GitHub).
- Кастомный дизайн UI логина.
- Реальные сертификаты для подписи JWT в production (сейчас dev-certs).

Всё это можно добавлять итеративно, не перестраивая базу.

---

## Production-готовность (must-do перед запуском)

1. **Сертификаты подписи**. В Development OpenIddict использует автогенерируемые сертификаты (`AddDevelopmentEncryptionCertificate` / `AddDevelopmentSigningCertificate`). Для прода нужны постоянные сертификаты, чтобы access tokens не инвалидировались при рестарте.

2. **HTTPS обязателен**. По умолчанию OpenIddict его требует.

3. **CORS на endpoint-ах** — настроить под домены SPA.

4. **Lockout policy** — настроить блокировку аккаунтов на N попыток в `Identity` options.

5. **Rate limiting** на `/connect/token` и `/connect/authorize`.

6. **Логирование событий аутентификации** в отдельный лог.