# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is a .NET 9 JWT authentication system with unique session management. The system enforces that only one device can be logged in per user at any time - when a user logs in from a new device, previous sessions are automatically invalidated.

## Architecture

The solution consists of 3 main projects:

- **JWTAuth/**: Main API server with JWT authentication, session management, and Entity Framework Core
- **ConsolaJWT/**: Console client application for testing the API endpoints
- **GeneradorHash/**: Utility to generate secure JWT signing keys using SHA-512

## Key Technologies

- .NET 9 with minimal APIs
- Entity Framework Core with SQL Server
- JWT Bearer authentication with custom session validation
- Custom middleware for session management
- Scalar for API documentation (OpenAPI)

## Common Development Commands

### Building and Running
```bash
# Build entire solution
dotnet build Security.JWTAuth.sln

# Restore dependencies
dotnet restore Security.JWTAuth.sln

# Run the main API (from JWTAuth directory)
dotnet run

# Run console client (from ConsolaJWT directory)
dotnet run

# Generate JWT key (from GeneradorHash directory)
dotnet run
```

### Database Operations
```bash
# Create and apply migration (from JWTAuth directory)
dotnet ef migrations add "Migration Name"
dotnet ef database update

# Drop database
dotnet ef database drop

# Check EF status
dotnet ef dbcontext info
dotnet ef migrations list
```

### API Testing
The API runs on port 5000 by default. Key endpoints:
- POST `/api/auth/register` - User registration
- POST `/api/auth/login` - User login (invalidates other sessions)
- POST `/api/auth/refresh-token` - Token refresh
- GET `/api/auth/session-info` - Current session information
- POST `/api/auth/logout` - Logout current session

## Critical Components

### Session Management
- **SessionValidationMiddleware**: Validates that the user's session is still active on each request
- **AuthService**: Handles login/logout and session invalidation logic
- **User Entity**: Stores SessionId, LastLoginTime, LastLoginIp for session tracking

### Security Features
- Passwords are hashed using ASP.NET Core Identity PasswordHasher
- JWT tokens have strict validation (ClockSkew = 0)
- Each login generates a unique SessionId that invalidates previous sessions
- Refresh tokens with expiration tracking

## Configuration

### Database Connection
Update `appsettings.json` ConnectionString for your SQL Server instance:
```json
"ConnectionStrings": {
  "UserDatabase": "Server=.;Database=UserDb;Trusted_Connection=true;TrustServerCertificate=true;"
}
```

### JWT Configuration
The JWT token, issuer, and audience are configured in `appsettings.json`. Use the GeneradorHash project to generate secure keys.

## Development Notes

- The system uses Entity Framework Code First approach
- Custom middleware runs after authentication but before authorization
- Session invalidation is handled at the database level by updating the SessionId
- The console client demonstrates session invalidation by allowing multiple instances to test the same user
- Token expiration is set to 1 minute by default for demo purposes (configurable in AuthService.cs:117)

## Testing Session Invalidation

1. Run the API: `dotnet run` (from JWTAuth directory)
2. Open two console clients: `dotnet run` (from ConsolaJWT directory)
3. Register a user in one client
4. Login with the same user in both clients
5. Observe that the first session becomes invalid when the second logs in