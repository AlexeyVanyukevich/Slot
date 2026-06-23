# Universal Booking Platform

## Database Migrations

### Prerequisites

Install the EF Core CLI tools (once per machine):

```powershell
dotnet tool install --global dotnet-ef
```

### UBP.IAM (Identity & Access Management)

Run the following commands from the repository root.

**Create a new migration:**

```powershell
dotnet ef migrations add <MigrationName> `
  --project UBP.IAM/UBP.IAM.Persistence `
  --startup-project UBP.IAM/UBP.IAM.API `
  --output-dir Migrations
```

**Apply migrations:**

```powershell
dotnet ef database update `
  --project UBP.IAM/UBP.IAM.Persistence `
  --startup-project UBP.IAM/UBP.IAM.API
```

> Migrations are automatically applied on startup in the Development environment.