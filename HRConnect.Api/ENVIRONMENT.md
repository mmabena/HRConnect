# Environment & Secrets (HRConnect.Api)

This file describes the environment variables used by the API. Production secrets must come from environment variables; local development may use `appsettings.Development.json` (this file is ignored by git).

Required environment variables (production):

- `JwtSettings__Secret` — the JWT signing secret (base64 recommended).
- `SendGrid__ApiKey` — SendGrid API key for sending email.

Optional non-sensitive settings (kept in `appsettings.json`): `JwtSettings:Issuer`, `JwtSettings:Audience`, `JwtSettings:ExpiryMinutes`, `SendGrid:FromEmail`, `SendGrid:FromName`.

Examples

PowerShell (session only):

```powershell
$env:JwtSettings__Secret = "<your-base64-or-utf8-secret>"
$env:SendGrid__ApiKey = "SG.xxxxxx"
dotnet run --project HRConnect.Api
```

PowerShell (persist for current user):

```powershell
setx JwtSettings__Secret "<your-base64-or-utf8-secret>"
setx SendGrid__ApiKey "SG.xxxxxx"
```

Linux / systemd / Docker (bash):

```bash
export JwtSettings__Secret="<your-base64-or-utf8-secret>"
export SendGrid__ApiKey="SG.xxxxxx"
dotnet run --project HRConnect.Api
```

dotnet user-secrets (local dev alternative):

```bash
cd HRConnect.Api
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "<secret>"
dotnet user-secrets set "SendGrid:ApiKey" "SG.xxxxx"
```

Notes

- `appsettings.Development.json` is present for local convenience and is excluded from source control.
- The application reads configuration from JSON files and environment variables following ASP.NET Core defaults (JSON then environment variables). Environment variables override JSON values.
