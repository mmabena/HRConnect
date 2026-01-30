# Login Troubleshooting Guide

## Issues Fixed

### 1. **CORS Configuration** ✓
- **Problem**: Frontend running on `localhost:3000` could not reach backend on `localhost:5147` due to restrictive CORS policy
- **Solution**: Updated `Program.cs` to allow both ports:
  ```csharp
  .WithOrigins("http://localhost:3000", "http://localhost:5147")
  .AllowAnyHeader()
  .AllowAnyMethod()
  .AllowCredentials()
  ```

### 2. **Token Storage Consistency** ✓
- **Problem**: SignIn component saved token to localStorage key `"user"`, but API interceptor looked for `"currentUser"`
- **Solution**: Updated `App.js` `handleLoginSuccess` to save both token and user to the correct key:
  ```javascript
  localStorage.setItem("currentUser", JSON.stringify({ token, user }));
  ```

### 3. **Enhanced Error Handling** ✓
- **Problem**: Unclear error messages made debugging difficult
- **Solution**: Added comprehensive console logging and better error parsing in SignIn component

## How to Test Login

### Prerequisites
1. **Backend Running**: Ensure `dotnet run` is executing at `C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Server\HRConnect.Api`
2. **Frontend Running**: Ensure `npm start` is running in `C:\Users\MMabena.SINGULAR\Desktop\HRConnect\Client`
3. **Database**: SQL Server must be running with the HRConnect database initialized
4. **User Account**: A test user must exist with a valid `@singular.co.za` email

### Steps to Test
1. Open browser DevTools (F12)
2. Go to Console tab
3. Navigate to login page
4. Enter valid email (e.g., `test@singular.co.za`) and password
5. Click Sign In
6. **Check console output** for detailed logs:
   - "Attempting login with email: ..."
   - "Response status: ..."
   - "Login successful" or error details

## Common Issues & Solutions

### Issue 1: Network Error / Server Connection Failed
**Error Message**: "Network error. Please check your connection and ensure the server is running."

**Troubleshooting**:
1. Verify backend is running: `http://localhost:5147/swagger/index.html` should load
2. Check if port 5147 is in use: `netstat -ano | findstr :5147`
3. Restart backend: Stop the process and run `dotnet run` again

### Issue 2: Invalid Email or Password (Even with Correct Credentials)
**Possible Causes**:
- User doesn't exist in database
- Email must be from `@singular.co.za` domain
- Password hasn't been set (null in database)

**Solution**:
1. Verify user exists in database:
   ```sql
   SELECT UserId, Email, PasswordHash FROM Users WHERE Email = 'test@singular.co.za'
   ```
2. If user doesn't exist, create one via User Management
3. Check PasswordHash is not null

### Issue 3: CORS Error (No Response from Server)
**Browser Console Error**: "Access to XMLHttpRequest ... blocked by CORS policy"

**Solution**:
- Ensure `Program.cs` has the correct CORS policy (already fixed)
- Clear browser cache: `Ctrl+Shift+Delete`
- Restart browser

### Issue 4: 401 Unauthorized
**Error**: "Invalid email or password."

**Check**:
1. Email and password are correct
2. Email is from `@singular.co.za` domain
3. User account is active in database

### Issue 5: Token Not Saved Properly
**Symptoms**: Login appears to succeed but redirects back to login page

**Debug**:
1. Open DevTools → Application → LocalStorage
2. Look for key: `currentUser`
3. Should contain: `{"token": "eyJ...", "user": {"id": ..., "email": "...", "role": "..."}}`
4. If missing or incorrect, check console for errors

## API Endpoint Details

### Login Endpoint
- **URL**: `POST http://localhost:5147/api/auth/login`
- **Request Body**:
  ```json
  {
    "email": "test@singular.co.za",
    "password": "YourPassword123!"
  }
  ```
- **Success Response (200)**:
  ```json
  {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "user": {
      "id": "user-guid",
      "email": "test@singular.co.za",
      "role": "NormalUser"
    }
  }
  ```
- **Error Response (400/401)**:
  ```json
  {
    "errors": ["Email must be from singular.co.za domain." | "Invalid email or password."]
  }
  ```

## Testing with Swagger UI

1. Navigate to: `http://localhost:5147/swagger/index.html`
2. Find the `/api/auth/login` endpoint
3. Click "Try it out"
4. Enter credentials in the request body:
   ```json
   {
     "email": "test@singular.co.za",
     "password": "YourPassword123!"
   }
   ```
5. Click "Execute"
6. Review response

## Files Modified

1. **[Program.cs](Server/HRConnect.Api/Program.cs)** - CORS configuration
2. **[App.js](Client/src/App.js)** - Token storage consistency
3. **[SignIn.jsx](Client/src/Components/SignIn.jsx)** - Enhanced error handling and logging

## Next Steps

- [ ] Test login with valid credentials
- [ ] Verify token is saved to localStorage
- [ ] Check that authenticated requests include Authorization header
- [ ] Test logout functionality
- [ ] Test password reset flow
