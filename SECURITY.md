# SECURITY — Secrets & Key Management 🔒

This file explains how we manage **secrets** (API keys, signing secrets) and what to do if a secret is exposed.

## Principle
- **Never** commit secrets (API keys, JWT secrets, private keys) to the repository. Use `dotnet user-secrets` for local development and environment variables in CI/production.

## Local development
- Use `dotnet user-secrets` (applies to the `HRConnect.Api` project):

  ```bash
  cd HRConnect.Api
  dotnet user-secrets init
  dotnet user-secrets set "JwtSettings:Secret" "<your-secret>"
  dotnet user-secrets set "SendGrid:ApiKey" "SG.xxxxx"
  ```

- Alternatively, set environment variables for a local session:

  PowerShell (session only):
  ```powershell
  $env:JwtSettings__Secret = "<your-secret>"
  $env:SendGrid__ApiKey = "SG.xxxxx"
  dotnet run --project HRConnect.Api
  ```

## Production / CI
- Store secrets in your platform's secret store (Azure Key Vault, GitHub Actions secrets, GitLab CI variables, etc.) and map them to environment variables:
  - `JwtSettings__Secret`
  - `SendGrid__ApiKey`

## If you find a secret in a local file
1. **Redact/delete the file locally**:
   ```powershell
   Remove-Item -Path Server\HRConnect.Api\appsettings.Development.json -Force
   ```
2. **Clear user-secrets (if used)**:
   ```bash
   cd HRConnect.Api
   dotnet user-secrets remove "SendGrid:ApiKey"
   dotnet user-secrets remove "JwtSettings:Secret"
   # or clear all
   dotnet user-secrets clear
   ```
3. **Rotate the exposed secret immediately** (create a new SendGrid key, new JWT secret, etc.).
4. If the secret was ever pushed publicly, purge it from git history and force-push (use `BFG` or `git filter-repo`) and notify stakeholders.

## Preventative measures
- Keep `appsettings.*.json` files ignored by git (already present in `.gitignore`).
- Add secret-scanning and pre-commit checks (e.g., `gitleaks`, `git-secrets`, `pre-commit`, `husky`).
- Enable GitHub/GitLab secret scanning and alerts on your remote repo.

## Scanning tips
- Quick local search for SendGrid/JWT secrets:
  ```powershell
  git grep -n "SG\." 2>$null || Write-Output "no matches"
  git grep -n "JwtSettings:Secret" 2>$null || Write-Output "no matches"
  ```

Following these rules keeps secrets out of the repository and helps reduce the blast radius if a secret is accidentally exposed.