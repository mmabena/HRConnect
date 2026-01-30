# HRConnect - Backend & Frontend Integration Guide

## Overview

HRConnect is an HR Management System with a C# ASP.NET Core backend and React frontend. This guide covers the complete setup and configuration for running both services locally.

---

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Backend Setup](#backend-setup)
3. [Frontend Setup](#frontend-setup)
4. [Authentication Flow](#authentication-flow)
5. [Running the Application](#running-the-application)
6. [Testing](#testing)
7. [Troubleshooting](#troubleshooting)

---

## Prerequisites

### Required Software
- **.NET 9** or later ([Download](https://dotnet.microsoft.com/download))
- **Node.js 16+** ([Download](https://nodejs.org/))
- **npm** or **yarn** (comes with Node.js)
- **SQL Server** or compatible database
- **Git** for version control
- **Visual Studio Code** or any code editor

### System Requirements
- Windows, macOS, or Linux
- Minimum 4GB RAM
- At least 2GB free disk space

---

## Backend Setup

### 1. Navigate to Backend Directory

```powershell
cd C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Server\HRConnect.Api
```

### 2. Restore NuGet Packages

```powershell
dotnet restore
```

### 3. Configure Database Connection

Update `appsettings.json` with your database connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=HRConnectDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "Issuer": "HRConnect",
    "Audience": "HRConnectUsers",
    "Key": "your-secret-key-at-least-32-characters-long"
  }
}
```

### 4. Apply Migrations

```powershell
dotnet ef database update
```

### 5. Build the Solution

```powershell
dotnet build
```

### 6. Run the Backend

```powershell
dotnet run
```

The API will be available at `http://localhost:5147`

---

## Frontend Setup

### 1. Navigate to Frontend Directory

```powershell
cd C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Client
```

### 2. Install Dependencies

```powershell
npm install
```

### 3. Configure API Base URL

The frontend is already configured to use `http://localhost:5037/api` in `src/api.js`. If your backend runs on a different port, update:

```javascript
const api = axios.create({
  baseURL: 'http://localhost:YOUR_PORT/api',
  headers: {
    'Content-Type': 'application/json',
  },
});
```

### 4. Start Development Server

```powershell
npm start
```

The frontend will open at `http://localhost:3000`

---

## Authentication Flow

### Login Flow

1. User enters email (must end with `@singular.co.za`) and password
2. Click "Sign In"
3. Frontend calls `POST /api/auth/login` with credentials
4. Backend validates and returns JWT token
5. Token is stored in `localStorage` as `currentUser`
6. User is redirected to `/dashboard`

**Request:**
```json
{
  "email": "user@singular.co.za",
  "password": "SecurePass123!"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 1,
    "email": "user@singular.co.za",
    "role": "NormalUser"
  }
}
```

### Forgot Password Flow

#### Step 1: Request PIN
- User clicks "Forgot Password?"
- Enters email address
- Frontend calls `POST /api/auth/forgot-password`
- Backend generates 4-digit PIN and sends via email
- PIN is valid for 1 minute

#### Step 2: Verify PIN
- User enters 4-digit PIN received in email
- Frontend calls `POST /api/auth/verify-pin`
- Backend validates PIN

#### Step 3: Reset Password
- User enters new password twice
- Password must meet requirements:
  - Minimum 8 characters
  - Contains uppercase letter (A-Z)
  - Contains lowercase letter (a-z)
  - Contains digit (0-9)
  - Contains special character (!@#$%^&*...)
- Frontend calls `POST /api/auth/reset-password`
- Backend updates password and marks PIN as used

**Forgot Password Request:**
```json
{
  "email": "user@singular.co.za"
}
```

**Verify PIN Request:**
```json
{
  "email": "user@singular.co.za",
  "pin": "1234"
}
```

**Reset Password Request:**
```json
{
  "email": "user@singular.co.za",
  "pin": "1234",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

---

## Running the Application

### Option 1: Run Both Services Simultaneously

**Terminal 1 - Backend:**
```powershell
cd C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Server\HRConnect.Api
dotnet run
```

**Terminal 2 - Frontend:**
```powershell
cd C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Client
npm start
```

### Option 2: Using npm Scripts (if configured)

In the frontend `package.json`, you can add:

```json
{
  "scripts": {
    "start": "react-scripts start",
    "backend": "cd ../Server/HRConnect.Api && dotnet run",
    "dev": "concurrently \"npm start\" \"npm run backend\""
  }
}
```

Then run:
```powershell
npm run dev
```

### Accessing the Application

- **Frontend:** http://localhost:3000
- **Backend API:** http://localhost:5147
- **API Documentation (Swagger):** http://localhost:5147/swagger

---

## Testing

### 1. Create Test User (Backend)

Run this SQL query on your database:

```sql
INSERT INTO Users (Email, PasswordHash, Role, CreatedAt)
VALUES ('test@singular.co.za', 'hashed_password_here', 'NormalUser', GETUTCDATE())
```

Or use the API endpoint to create a user if available.

### 2. Test Login Flow

1. Open http://localhost:3000
2. Enter email: `test@singular.co.za`
3. Enter password: (the password set above)
4. Click "Sign In"
5. Should redirect to Dashboard

### 3. Test Forgot Password Flow

1. Click "Forgot Password?"
2. Enter email: `test@singular.co.za`
3. Check backend logs or email for 4-digit PIN
4. Enter PIN (in dev mode, check console/logs)
5. Enter new password meeting all requirements
6. Confirm password
7. Click "Reset Password"
8. Should redirect to login page

### 4. API Testing with Postman/cURL

**Login Test:**
```bash
curl -X POST http://localhost:5147/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@singular.co.za","password":"TestPass123!"}'
```

**Forgot Password Test:**
```bash
curl -X POST http://localhost:5147/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"test@singular.co.za"}'
```

---

## Troubleshooting

### Backend Issues

#### Port Already in Use
```powershell
# Find process using port 5147
Get-NetTCPConnection -LocalPort 5147

# Kill the process
Stop-Process -Id <PID> -Force
```

#### Database Connection Error
- Verify connection string in `appsettings.json`
- Ensure SQL Server is running
- Check database exists
- Run migrations: `dotnet ef database update`

#### JWT Configuration Error
- Ensure JWT secret key is at least 32 characters
- Verify Issuer and Audience match in `appsettings.json`

### Frontend Issues

#### Dependencies Not Installing
```powershell
# Clear npm cache
npm cache clean --force

# Delete node_modules and package-lock.json
rm -r node_modules package-lock.json

# Reinstall
npm install
```

#### API Connection Error
- Ensure backend is running on port 5147
- Check `src/api.js` baseURL
- Look for CORS issues in browser console
- Verify firewall allows localhost traffic

#### Port 3000 Already in Use
```powershell
# Find process using port 3000
netstat -ano | findstr :3000

# Kill the process
taskkill /PID <PID> /F

# Or use different port
PORT=3001 npm start
```

### General Issues

#### ESLint Warnings
These are non-critical and won't prevent the app from running. To suppress:
- Add `// eslint-disable-next-line` comment above warning lines
- Or configure `.eslintignore` file

#### Git Merge Conflicts
```powershell
# If you encounter conflicts during pull:
git status  # See conflicted files
git add .
git commit -m "Resolve merge conflicts"
```

---

## Project Structure

```
HRConnect/
├── Server/
│   └── HRConnect.Api/
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── UserController.cs
│       │   └── ...
│       ├── Services/
│       ├── Models/
│       ├── Data/
│       ├── appsettings.json
│       └── Program.cs
└── Client/
    └── HRConnect-FrontEnd/
        ├── src/
        │   ├── Components/
        │   │   ├── SignIn/
        │   │   ├── ForgotPassword/
        │   │   ├── Services/
        │   │   │   └── authService.js
        │   │   └── ...
        │   ├── api.js
        │   └── App.js
        ├── package.json
        └── ...
```

---

## Key Files

### Backend
- **AuthController.cs** - Handles login, forgot-password, verify-pin, reset-password endpoints
- **UserRepository.cs** - Database access for user operations
- **Program.cs** - API configuration and startup

### Frontend
- **authService.js** - Centralized API calls for authentication
- **SignIn.jsx** - Login page component
- **ForgotPassword.jsx** - Multi-step password reset component
- **api.js** - Axios instance with JWT interceptor
- **App.js** - Main app routing and state management

---

## Security Considerations

### In Production

1. **JWT Secret:** Use a strong, randomly generated key (min 32 characters)
2. **HTTPS:** Always use HTTPS in production
3. **CORS:** Configure CORS to allow only trusted origins
4. **Token Expiry:** Implement token expiration and refresh logic
5. **Password Storage:** Passwords are hashed with BCrypt (already implemented)
6. **Email Verification:** Consider adding email verification for security
7. **Rate Limiting:** Implement rate limiting on authentication endpoints
8. **Logging:** Log authentication attempts and failures

### Current Implementation
- ✅ Passwords hashed with BCrypt
- ✅ JWT token-based authentication
- ✅ 4-digit PIN for password reset (1-minute expiry)
- ✅ Email domain validation (@singular.co.za)
- ✅ Password complexity requirements
- ✅ Prevents password reuse (history tracking)

---

## Useful Commands

### Backend
```powershell
# Build
dotnet build

# Run
dotnet run

# Watch mode (auto-restart on changes)
dotnet watch run

# Create migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# View logs
dotnet run --verbosity detailed
```

### Frontend
```powershell
# Install dependencies
npm install

# Start dev server
npm start

# Build for production
npm run build

# Run tests
npm test

# Lint code
npm run lint

# Eject configuration (one-way, be careful!)
npm run eject
```

### Git
```powershell
# Check status
git status

# Commit changes
git add .
git commit -m "Your message"

# Push to remote
git push origin master

# Pull latest changes
git pull origin master

# View commit history
git log --oneline
```

---

## Support & Next Steps

### Recommended Enhancements
1. Implement 2FA (Two-Factor Authentication)
2. Add password strength meter
3. Add "Remember Me" functionality
4. Implement OAuth/SSO integration
5. Add audit logging for security events
6. Implement email notification system
7. Add rate limiting and brute-force protection

### Contact & Documentation
For issues or questions:
- Check browser console for frontend errors
- Check backend terminal/logs for API errors
- Use `git log` to review recent changes
- Enable debug mode for detailed logging

---

**Last Updated:** December 9, 2025  
**Version:** 1.0  
**Status:** Ready for Development
