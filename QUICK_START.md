# HRConnect - Quick Start Guide

## 5-Minute Setup

### Prerequisites
- .NET 9+ installed
- Node.js 16+ installed
- SQL Server running
- Git installed

---

## Start Backend

```powershell
cd C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Server\HRConnect.Api
dotnet run
```

**Expected output:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5147
```

API will be available at: `http://localhost:5147/swagger`

---

## Start Frontend

```powershell
cd C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Client
npm start
```

**Expected output:**
```
On Your Network:  http://192.168.x.x:3000
```

App will open at: `http://localhost:3000`

---

## Test Login

1. **Create a test user** (if you don't have one):
   - Use the backend API to create a user
   - Or add directly to database with your own credentials

2. **Go to login page:**
   - Navigate to http://localhost:3000
   - You'll see the login screen

3. **Enter credentials:**
   - Email: `test@singular.co.za` (or your test email)
   - Password: (your test password)
   - Click "Sign In"

4. **Expected result:**
   - Redirected to `/dashboard`
   - User data stored in browser localStorage

---

## Test Forgot Password

1. **Click "Forgot Password?"** on login page

2. **Step 1 - Enter Email:**
   - Email: `test@singular.co.za`
   - Click "Send PIN"
   - Check backend logs for PIN (appears in console)

3. **Step 2 - Verify PIN:**
   - Enter 4-digit PIN from logs
   - Click "Verify PIN"

4. **Step 3 - Reset Password:**
   - New password: `NewSecure123!` (must meet requirements)
   - Confirm: `NewSecure123!`
   - Click "Reset Password"

5. **Expected result:**
   - Redirected to login page
   - Can login with new password

---

## Password Requirements

Password must have:
- At least 8 characters
- Uppercase letter (A-Z)
- Lowercase letter (a-z)
- Digit (0-9)
- Special character (!@#$%^&*...)

**Valid example:** `SecurePass123!`  
**Invalid example:** `password123` (no uppercase, no special char)

---

## Key URLs

| Service | URL | Purpose |
|---------|-----|---------|
| Frontend | http://localhost:3000 | React app |
| Backend API | http://localhost:5147/api | API endpoints |
| Swagger Docs | http://localhost:5147/swagger | API documentation |

---

## Key Files

| File | Purpose |
|------|---------|
| `Client/src/Components/Services/authService.js` | API calls |
| `Client/src/Components/SignIn/SignIn.jsx` | Login page |
| `Client/src/Components/ForgotPassword/ForgotPassword.jsx` | Password reset |
| `Client/src/api.js` | Axios configuration |
| `Client/src/App.js` | Routing |

---

## Quick Troubleshooting

### Port Already in Use
```powershell
# Kill process on port 5147 (backend)
Get-NetTCPConnection -LocalPort 5147 | Stop-Process -Force

# Kill process on port 3000 (frontend)
Get-NetTCPConnection -LocalPort 3000 | Stop-Process -Force
```

### Dependencies Missing
```powershell
# Frontend
cd Client
npm install

# Backend
cd ../Server/HRConnect.Api
dotnet restore
```

### API Not Responding
1. Check backend is running: `http://localhost:5147/swagger`
2. Check firewall allows localhost traffic
3. Restart both applications

### Database Error
1. Verify connection string in `appsettings.json`
2. Ensure SQL Server is running
3. Run migrations: `dotnet ef database update`

---

## More Documentation

For detailed information, see:
- **Setup Guide:** `Client/AUTHENTICATION_SETUP.md`
- **API Reference:** `Server/HRConnect.Api/AUTHENTICATION_API.md`
- **Full Summary:** `IMPLEMENTATION_SUMMARY.md`

---

## Local secrets with `dotnet user-secrets`

For local development you can store secrets in the per-user `dotnet user-secrets` store (these values do not get checked into source control).

1. Open a terminal in the API project folder:

```powershell
cd Server/HRConnect.Api
```

2. Initialize user-secrets (one-time per project):

```powershell
dotnet user-secrets init
```

3. Set secrets used by the application (examples):

```powershell
dotnet user-secrets set "JwtSettings:Secret" "<your-secret-or-base64>"
dotnet user-secrets set "SendGrid:ApiKey" "SG.xxxxx"
```

List stored secrets:

```powershell
dotnet user-secrets list
```

Notes:
- Keys use configuration naming (colon-delimited) and map to `IConfiguration`/`IOptions<T>` in the app.
- In production, set environment variables `JwtSettings__Secret` and `SendGrid__ApiKey` instead of using user-secrets.


---

## What's Working

- Login with email/password  
- JWT token storage and retrieval  
- 3-step forgot password flow  
- PIN verification  
- Password reset with strength requirements  
- Auto logout on token expiry  
- Responsive design  

---

## Security Notes

- Passwords are hashed with BCrypt
- JWT tokens are used for authentication
- PINs expire after 1 minute
- Email domain validation (@singular.co.za)
- Password history prevents reuse

---

## Need Help?

1. **Check the logs:**
   - Frontend: Browser console (F12)
   - Backend: Terminal output

2. **Read the docs:**
   - AUTHENTICATION_SETUP.md (frontend)
   - AUTHENTICATION_API.md (backend)

3. **Test the API:**
   - Use Swagger UI: http://localhost:5147/swagger
   - Or Postman: import requests from documentation

---

**Last Updated:** December 9, 2025  
**Version:** 1.0
