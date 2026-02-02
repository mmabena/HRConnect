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

## Protecting secrets (recommended) 🔒

- Use `dotnet user-secrets` for local development instead of committing secrets to JSON files:

  ```bash
  cd HRConnect.Api
  dotnet user-secrets init
  dotnet user-secrets set "JwtSettings:Secret" "<your-secret>"
  dotnet user-secrets set "SendGrid:ApiKey" "SG.xxxxx"
  ```

- Prefer environment variables in CI/production. Examples:

  PowerShell (session):
  ```powershell
  $env:JwtSettings__Secret = "<your-secret>"
  $env:SendGrid__ApiKey = "SG.xxxxx"
  dotnet run --project HRConnect.Api
  ```

  PowerShell (persist for current user):
  ```powershell
  setx JwtSettings__Secret "<your-secret>"
  setx SendGrid__ApiKey "SG.xxxxx"
  ```

- If you find secrets in local files:
  - **Delete or redact** the local file (PowerShell):
    ```powershell
    Remove-Item -Path Server\HRConnect.Api\appsettings.Development.json -Force
    ```
  - **Clear user-secrets** if you stored them there:
    ```bash
    cd HRConnect.Api
    dotnet user-secrets remove "SendGrid:ApiKey"
    dotnet user-secrets remove "JwtSettings:Secret"
    # or to clear all local secrets
    dotnet user-secrets clear
    ```
  - **Rotate keys** immediately if a secret was ever exposed publicly (e.g., create a new SendGrid API key and update environment/CICD secrets).

- Scan the repository for accidental secrets:
  ```powershell
  git grep -n "SG\." || Write-Output "no matches"
  git grep -n "JwtSettings:Secret" || Write-Output "no matches"
  ```
  Consider using secret scanners such as `gitleaks`, `truffleHog`, or GitHub's secret scanning for stronger checks.

- Add automated protection:
  - Keep `appsettings.*.json` files ignored by git (already in `.gitignore`).
  - Add pre-commit checks (`pre-commit`, `husky`, `git-secrets`) to detect secrets before pushing.
  - Enable GitHub repository secret scanning and alerts where available.

Following these steps ensures secrets are not stored in the repo and are safely managed in local dev and production.

