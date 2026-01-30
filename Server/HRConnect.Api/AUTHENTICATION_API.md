# HRConnect Backend - Authentication API Documentation

## Overview

This document describes the authentication API endpoints for the HRConnect backend (C# ASP.NET Core).

---

## API Base URL

```
http://localhost:5147/api
```

## Authentication Endpoints

### 1. Login

**Endpoint:** `POST /auth/login`

**Description:** Authenticate user with email and password

**Request Body:**
```json
{
  "email": "user@singular.co.za",
  "password": "SecurePassword123!"
}
```

**Response (200 OK):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "userId": 1,
    "email": "user@singular.co.za",
    "role": "NormalUser",
    "createdAt": "2024-12-09T10:00:00Z"
  },
  "message": "Login successful"
}
```

**Error Responses:**
- **400 Bad Request:** Email must be from singular.co.za domain
- **401 Unauthorized:** Invalid email or password
- **500 Internal Server Error:** Server error

---

### 2. Forgot Password (Request PIN)

**Endpoint:** `POST /auth/forgot-password`

**Description:** Request a password reset PIN via email

**Request Body:**
```json
{
  "email": "user@singular.co.za"
}
```

**Response (200 OK):**
```json
{
  "message": "PIN sent to your email.",
  "pin": "1234",
  "expiresAt": "2024-12-09T10:01:00Z"
}
```

**Note:** In production, the `pin` field should not be returned. It's included here for testing purposes.

**Error Responses:**
- **400 Bad Request:** Invalid email format or not from @singular.co.za domain
- **404 Not Found:** User with this email not found
- **500 Internal Server Error:** Failed to send email

---

### 3. Verify PIN

**Endpoint:** `POST /auth/verify-pin`

**Description:** Verify the 4-digit PIN sent to email

**Request Body:**
```json
{
  "email": "user@singular.co.za",
  "pin": "1234"
}
```

**Response (200 OK):**
```json
{
  "message": "PIN verified. You can now reset your password."
}
```

**Error Responses:**
- **400 Bad Request:** Invalid or expired PIN
- **400 Bad Request:** Invalid email format
- **500 Internal Server Error:** Verification failed

---

### 4. Reset Password

**Endpoint:** `POST /auth/reset-password`

**Description:** Reset password using verified PIN

**Request Body:**
```json
{
  "email": "user@singular.co.za",
  "pin": "1234",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

**Response (200 OK):**
```json
{
  "message": "Password reset successfully."
}
```

**Password Requirements:**
- Minimum 8 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (!@#$%^&*(),.?"{}\|<>)

**Error Responses:**
- **400 Bad Request:** 
  - Invalid or expired PIN
  - Password does not meet complexity requirements
  - Passwords do not match
  - Cannot reuse a previously used password
- **400 Bad Request:** Invalid email format
- **404 Not Found:** User not found
- **500 Internal Server Error:** Password reset failed

---

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=HRConnectDb;Trusted_Connection=true;"
  },
  "Jwt": {
    "Issuer": "HRConnect",
    "Audience": "HRConnectUsers",
    "Key": "your-super-secret-key-that-is-at-least-32-characters-long"
  },
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "your-email@gmail.com",
    "SenderPassword": "your-app-password"
  }
}
```

### Email Configuration

The system uses SMTP to send password reset PINs. Update the email settings in `appsettings.json`:

**For Gmail:**
1. Enable 2FA on your Gmail account
2. Generate App Password
3. Use the 16-character password in `SenderPassword`

**For Other Providers:**
- Adjust `SmtpServer` and `SmtpPort`
- Update sender credentials

---

## Database Schema

### Users Table

```sql
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(50),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

### PasswordResets Table

```sql
CREATE TABLE PasswordResets (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Pin NVARCHAR(4) NOT NULL,
    IsUsed BIT DEFAULT 0,
    ExpiresAt DATETIME2 NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
```

### PasswordHistory Table

```sql
CREATE TABLE PasswordHistory (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
    FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
```

---

## Security Features

### Implemented
- ✅ **Password Hashing:** BCrypt with salt
- ✅ **JWT Authentication:** Token-based stateless auth
- ✅ **PIN Expiry:** 1-minute validity for reset PINs
- ✅ **Password History:** Prevents reusing old passwords
- ✅ **Email Validation:** Domain restriction (@singular.co.za)
- ✅ **Password Strength:** Complexity requirements enforced

### Not Yet Implemented
- ⚠️ **Rate Limiting:** No limit on login/reset attempts
- ⚠️ **Account Lockout:** No temporary lockout after failed attempts
- ⚠️ **2FA:** No two-factor authentication
- ⚠️ **Email Verification:** No email confirmation for new accounts
- ⚠️ **Audit Logging:** No logging of authentication events

---

## Testing

### Using Postman

1. **Create Collection:** HRConnect Auth
2. **Add Environment Variables:**
   - `base_url`: http://localhost:5147/api
   - `token`: (leave empty, will be populated on login)

3. **Create Requests:**

**Login Request:**
```
POST {{base_url}}/auth/login
Body (JSON):
{
  "email": "test@singular.co.za",
  "password": "Test123!Pass"
}
```

**Forgot Password Request:**
```
POST {{base_url}}/auth/forgot-password
Body (JSON):
{
  "email": "test@singular.co.za"
}
```

**Verify PIN Request:**
```
POST {{base_url}}/auth/verify-pin
Body (JSON):
{
  "email": "test@singular.co.za",
  "pin": "1234"
}
```

**Reset Password Request:**
```
POST {{base_url}}/auth/reset-password
Body (JSON):
{
  "email": "test@singular.co.za",
  "pin": "1234",
  "newPassword": "NewTest123!Pass",
  "confirmPassword": "NewTest123!Pass"
}
```

### Using cURL

```bash
# Login
curl -X POST http://localhost:5147/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"test@singular.co.za","password":"Test123!Pass"}'

# Forgot Password
curl -X POST http://localhost:5147/api/auth/forgot-password \
  -H "Content-Type: application/json" \
  -d '{"email":"test@singular.co.za"}'

# Verify PIN
curl -X POST http://localhost:5147/api/auth/verify-pin \
  -H "Content-Type: application/json" \
  -d '{"email":"test@singular.co.za","pin":"1234"}'

# Reset Password
curl -X POST http://localhost:5147/api/auth/reset-password \
  -H "Content-Type: application/json" \
  -d '{"email":"test@singular.co.za","pin":"1234","newPassword":"NewTest123!Pass","confirmPassword":"NewTest123!Pass"}'
```

---

## Error Handling

All errors follow a standard format:

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "detail": "Email must be a @singular.co.za address.",
  "instance": "/api/auth/login"
}
```

Common HTTP Status Codes:
- `200 OK` - Request successful
- `400 Bad Request` - Invalid input or business logic error
- `401 Unauthorized` - Invalid credentials
- `404 Not Found` - Resource not found
- `500 Internal Server Error` - Server error

---

## Best Practices

### For Frontend Integration

1. **Store Token Securely:**
   ```javascript
   localStorage.setItem('currentUser', JSON.stringify({
     token: response.token,
     user: response.user
   }));
   ```

2. **Attach Token to Requests:**
   ```javascript
   api.interceptors.request.use((config) => {
     const token = localStorage.getItem('token');
     if (token) {
       config.headers.Authorization = `Bearer ${token}`;
     }
     return config;
   });
   ```

3. **Handle Token Expiry:**
   ```javascript
   api.interceptors.response.use(
     response => response,
     error => {
       if (error.response?.status === 401) {
         localStorage.removeItem('token');
         window.location.href = '/login';
       }
       return Promise.reject(error);
     }
   );
   ```

### For Backend Security

1. **Use Strong JWT Secret:**
   - Min 32 characters
   - Random and unique
   - Never hardcode in source

2. **Validate All Inputs:**
   - Email format
   - Password complexity
   - Email domain

3. **Log Authentication Events:**
   - Login attempts (success/failure)
   - PIN requests
   - Password resets

4. **Rate Limiting:**
   - Limit login attempts per IP
   - Limit password reset requests
   - Implement exponential backoff

---

## Troubleshooting

### Email Not Being Sent
- Check SMTP credentials in appsettings.json
- Verify firewall allows SMTP port (usually 587)
- Check email logs in application
- Ensure sender email is valid

### PIN Expired
- PIN is valid for 1 minute
- User must request new PIN if expired
- Check system clock synchronization

### Password Validation Fails
- Ensure password meets all requirements
- Min 8 chars, uppercase, lowercase, digit, special char
- Verify special characters are supported

### Database Connection Error
- Verify connection string
- Ensure SQL Server is running
- Check database exists
- Verify user has permissions

---

## Maintenance

### Regular Tasks

**Monthly:**
- Review authentication logs
- Check password reset requests
- Monitor failed login attempts
- Update security patches

**Quarterly:**
- Audit user accounts
- Review password policies
- Update dependencies
- Security assessment

---

**Last Updated:** December 9, 2025  
**Version:** 1.0  
**Status:** Production Ready
