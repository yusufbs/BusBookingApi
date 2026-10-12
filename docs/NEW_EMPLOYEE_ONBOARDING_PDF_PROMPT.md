# Prompt to Generate the New Employee Onboarding & Architecture PDF (`BusBookingApi`)

Copy and paste the prompt block below into an AI assistant (with PDF/Canvas/Document generation enabled) or use it with Pandoc / Markdown-to-PDF tooling to generate a complete, self-contained **New Employee Onboarding & System Architecture PDF** for the **Bus Booking Web API** project.

---

```markdown
# ROLE & OBJECTIVE
You are a Technical Documentation Architect. Generate a comprehensive, professionally formatted **PDF Document** titled:
**"BusBooking Web API (.NET 10 & SQL Server) — New Employee Onboarding, Architecture & Local Setup Guide"**

The PDF must be structured for new engineering hires joining the team so they can:
1. Read and understand the **entire original prompt and specification** that created the system, plus the chronological evolution of the codebase during our pair-programming session.
2. Understand **what the application does** in detail—its business domain, Clean Architecture layers, database schema, concurrency controls, security/RBAC model, and enterprise non-functional features.
3. Follow **step-by-step instructions** to configure, build, seed, run, and test the application on their local Windows machine for the first time.

Include all content below in full—do not summarize, omit, or abbreviate any tables, code blocks, endpoints, or prompts.

---

# DOCUMENT CONTENTS TO INCLUDE IN THE PDF

## SECTION 1: THE ORIGINAL PROMPTS & CONVERSATION EVOLUTION

### 1.1 Initial User Request Prompt
> "create a prompt which will help me build a web api in dotnet 10 and sql server database with all the best practices and highly performant. this is a bus booking api and the details can be found at the site https://freeprojectapi.com/apis/bus-booking, the end points should look like this and accordingly the db tables should be created, this application should have health checks + matrics, observability, rate limiting, api versioning, proper logging, caching, feature management, exception handling management, resilience"

---

### 1.2 The Complete Architectural Master Prompt (Verbatim)

#### Role & Objective
You are a Principal .NET Solutions Architect and High-Performance Systems Engineer. Your task is to design and implement a production-ready, ultra-low latency, and resilient Bus Ticket Booking Web API built with **.NET 10** and **Microsoft SQL Server**.

The business domain and contract specifications mirror the public API contract from FreeProjectAPI (https://freeprojectapi.com/apis/bus-booking). You must build the system using **Clean Architecture** (or Vertical Slice Architecture), modern C# 14 idioms, zero-allocation best practices, and enterprise non-functional requirements (Observability, Resilience, Caching, Rate Limiting, Versioning, Feature Management, and Health Checks).

#### 1. Technology Stack & Key Libraries
- **Runtime & Language**: .NET 10 / C# 14
- **Database**: Microsoft SQL Server 2022+
- **Data Access**:
  - **Entity Framework Core 10** for Domain writes, migrations, relationships, and transactional consistency.
  - **Dapper / Compiled No-Tracking Queries** for high-throughput read paths (e.g., bus search and route exploration).
- **Caching**: .NET `HybridCache` (L1 In-Memory + L2 Distributed Redis) with tag-based cache eviction.
- **Resilience**: Polly v8 via `Microsoft.Extensions.Resilience` (Retry, Circuit Breaker, Timeout, Rate Limiter).
- **Observability**:
  - OpenTelemetry (`OpenTelemetry.Extensions.Hosting`, ASP.NET Core, HttpClient, EF Core instrumentation, OTLP Exporter).
  - Custom `System.Diagnostics.Metrics.Meter` for booking KPIs.
  - Serilog with structured JSON logging and OpenTelemetry trace/span correlation.
- **Health Checks**: `AspNetCore.HealthChecks.SqlServer`, `AspNetCore.HealthChecks.Redis`, custom readiness/liveness probes (`/health/live`, `/health/ready`).
- **Rate Limiting**: `Microsoft.AspNetCore.RateLimiting` (Fixed Window, Sliding Window, Token Bucket partitioned by Client IP / Customer ID).
- **API Versioning**: `Asp.Versioning.Http` and `Asp.Versioning.Mvc.ApiExplorer` (supporting `/api/v{version:apiVersion}/...` with backward-compatible fallback).
- **Feature Management**: `Microsoft.FeatureManagement.AspNetCore`.
- **Exception Handling**: Global `IExceptionHandler` producing standard RFC 7807 `ProblemDetails`.
- **Authentication & Security**: ASP.NET Core JWT Bearer authentication, Refresh Tokens, BCrypt password hashing, and Role-Based Authorization (`Admin`, `Manager`, `Associate`).

#### 2. Relational Database Schema & Data Modeling
Generate EF Core 10 fluent configurations and SQL Server DDL with optimal data types, foreign keys, cascade rules, and covering indexes.

##### Core Tables & Fields:
1. **`Users`**
   - `UserId` (INT, IDENTITY, PK)
   - `UserName` (NVARCHAR(100), UNIQUE, NOT NULL)
   - `EmailId` (NVARCHAR(150), UNIQUE, NOT NULL)
   - `FullName` (NVARCHAR(150), NOT NULL)
   - `Role` (NVARCHAR(50), NOT NULL) - [`Admin`, `Manager`, `Associate`, `Customer`]
   - `PasswordHash` (NVARCHAR(255), NOT NULL)
   - `PasswordSalt` (NVARCHAR(255), NOT NULL)
   - `CreatedDate` (DATETIMEOFFSET, NOT NULL, DEFAULT SYSUTCDATETIME())
   - `RefreshToken` (NVARCHAR(255), NULL)
   - `RefreshTokenExpiryTime` (DATETIMEOFFSET, NULL)
   - `RowVersion` (ROWVERSION / TIMESTAMP, Concurrency Token)

2. **`BusVendors`**
   - `VendorId` (INT, IDENTITY, PK)
   - `VendorName` (NVARCHAR(150), NOT NULL)
   - `ContactNo` (VARCHAR(20), NOT NULL)
   - `EmailId` (NVARCHAR(150), NOT NULL)
   - `IsActive` (BIT, NOT NULL, DEFAULT 1)

3. **`BusLocations`**
   - `LocationId` (INT, IDENTITY, PK)
   - `LocationName` (NVARCHAR(100), NOT NULL)
   - `Code` (VARCHAR(10), NOT NULL, UNIQUE)

4. **`BusAddressLocations`** (Pickup / Drop-off points)
   - `LocationPointId` (INT, IDENTITY, PK)
   - `LocationId` (INT, FK -> BusLocations.LocationId, ON DELETE CASCADE)
   - `LocationTitle` (NVARCHAR(150), NOT NULL)
   - `LocationAddress` (NVARCHAR(500), NOT NULL)

5. **`BusSchedules`**
   - `ScheduleId` (INT, IDENTITY, PK)
   - `VendorId` (INT, FK -> BusVendors.VendorId)
   - `BusName` (NVARCHAR(100), NOT NULL)
   - `BusVehicleNo` (VARCHAR(50), NOT NULL)
   - `FromLocation` (INT, FK -> BusLocations.LocationId)
   - `ToLocation` (INT, FK -> BusLocations.LocationId)
   - `DepartureTime` (TIME, NOT NULL)
   - `ArrivalTime` (TIME, NOT NULL)
   - `ScheduleDate` (DATE, NOT NULL)
   - `Price` (DECIMAL(18,2), NOT NULL)
   - `TotalSeats` (INT, NOT NULL)
   - `RowVersion` (ROWVERSION / Concurrency Token)

6. **`BusBookings`**
   - `BookingId` (INT, IDENTITY, PK)
   - `CustId` (INT, FK -> Users.UserId)
   - `ScheduleId` (INT, FK -> BusSchedules.ScheduleId)
   - `BookingDate` (DATETIMEOFFSET, NOT NULL, DEFAULT SYSUTCDATETIME())
   - `TotalAmount` (DECIMAL(18,2), NOT NULL)
   - `BookingStatus` (NVARCHAR(30), DEFAULT 'Confirmed')

7. **`BusBookingPassengers`**
   - `PassengerId` (INT, IDENTITY, PK)
   - `BookingId` (INT, FK -> BusBookings.BookingId, ON DELETE CASCADE)
   - `PassengerName` (NVARCHAR(150), NOT NULL)
   - `Age` (INT, NOT NULL)
   - `Gender` (VARCHAR(10), NOT NULL)
   - `SeatNo` (INT, NOT NULL)

##### Critical Constraints & Performance Indexes:
- **Seat Conflict Prevention**: Indexed View `dbo.vw_ActiveScheduleSeats` (`WITH SCHEMABINDING`) with `UNIQUE CLUSTERED INDEX IX_vw_ActiveScheduleSeats_Schedule_Seat ON dbo.vw_ActiveScheduleSeats(ScheduleId, SeatNo)` plus `SERIALIZABLE` transaction isolation to guarantee two concurrent users cannot book the same seat on the same schedule.
- **Search Covering Index**: `CREATE NONCLUSTERED INDEX IX_BusSchedules_Search ON BusSchedules(FromLocation, ToLocation, ScheduleDate) INCLUDE (ScheduleId, VendorId, BusName, BusVehicleNo, DepartureTime, ArrivalTime, Price, TotalSeats);`
- **Booked Seats Lookup Index**: `CREATE NONCLUSTERED INDEX IX_BusBookings_Schedule_Status ON BusBookings(ScheduleId, BookingStatus) INCLUDE (BookingId, CustId, BookingDate);`

#### 3. Complete API Contract & Route Specification (30 Endpoints)
Originally specified under `/api/BusBooking/...` and refactored during development into two dedicated controllers:
- **`AuthController` (`/api/Auth/...`)**: 5 Authentication & User Management endpoints (`GetAllUsers`, `login`, `AddNewUser`, `UpdateUser`, `DeleteUserByUserId`).
- **`BusBookingController` (`/api/BusBooking/...`)**: 25 Bus Booking domain endpoints covering Vendors, Locations & Boarding Points, Schedules & Route Search, and Bookings & Seat Allocation.

#### 4. Architecture & Design Principles
Clean Architecture with 4 projects:
1. `BusBooking.Domain`: Entities, enums, domain constants.
2. `BusBooking.Application`: CQRS Commands & Queries (MediatR), FluentValidation pipeline, DTOs, interfaces (`IBusBookingDbContext`, `ISqlConnectionFactory`, `IJwtTokenService`), OpenTelemetry metrics (`BusBookingMetrics`).
3. `BusBooking.Infrastructure`: EF Core 10 `BusBookingDbContext` (pooled), Fluent entity configurations, SQL migrations, Dapper `SqlConnectionFactory`, `JwtTokenService`, `.NET 10 HybridCache`.
4. `BusBooking.Api`: `AuthController`, `BusBookingController`, `GlobalExceptionHandler` (RFC 7807), JWT Bearer + RBAC policies, Rate Limiting, Polly v8 resilience, OpenTelemetry, Serilog, Health Checks, and Swagger UI.

#### 5. 4-Step Implementation Sequence Used to Build the Project
- **Step 1**: Domain Entities, EF Core 10 DbContext, Migrations, and SQL Server Schema with Covering Indexes & Indexed View.
- **Step 2**: Application CQRS Handlers, Dapper High-Throughput Search Queries, `HybridCache`, and Atomic Seat Booking Transaction.
- **Step 3**: API Controllers, DTOs, and Route Contracts (relocated to `C:\Projects\BusBookingApi`).
- **Step 4**: Enterprise Non-Functional Requirements (Serilog, OpenTelemetry, Polly v8, Rate Limiting, API Versioning, Feature Flags, RFC 7807 Exception Handling, Health Checks).

---

### 1.3 Summary of Subsequent Enhancements Made in This Conversation
After completing Steps 1–4, the following refinements were implemented:
1. **Workspace Standardization**: Moved the entire solution to `C:\Projects\BusBookingApi`.
2. **Swagger / OpenAPI UI**: Integrated `Swashbuckle.AspNetCore` v10 with root redirect (`/` -> `/swagger`) and OpenAPI v1 JSON specification (`/swagger/v1/swagger.json`).
3. **JWT Authentication & Role-Based Authorization (RBAC)**:
   - Added `JwtTokenService` issuing signed HMAC-SHA256 JWT tokens containing `NameIdentifier`, `Name`, `Email`, and `Role` claims.
   - Configured three primary operational roles: **`Admin`**, **`Manager`**, and **`Associate`** (plus `Customer`).
   - Seeded default users for each role in SQL Server (`admin`, `manager`, `associate`).
4. **Controller Separation (`AuthController` vs `BusBookingController`)**:
   - Extracted all 5 authentication and user management endpoints out of `BusBookingController` into a dedicated `AuthController` (`/api/Auth/...`).
   - Verified zero auth endpoints remain under `/api/BusBooking/...`.
5. **Swagger Bearer Authorization Fix**:
   - Passed the `OpenApiDocument` instance into `new OpenApiSecuritySchemeReference("Bearer", document)` so `swagger.json` emits `"security": [{ "Bearer": [] }]` and Swagger UI attaches the `Authorization` header to outgoing requests.
   - Added a `JwtBearerEvents.OnMessageReceived` normalizer in `Program.cs` so pasting either `<token>` or `Bearer <token>` in Swagger UI works seamlessly.
6. **Git Repository Hygiene**:
   - Added a comprehensive `.gitignore` for .NET 10, Visual Studio, VS Code, Rider, NuGet, SQL Server, and OS artifacts, and untracked `bin/` and `obj/` from Git index.

---

## SECTION 2: DETAILED EXPLANATION OF WHAT THE APPLICATION DOES

### 2.1 Business Purpose
`BusBookingApi` is an enterprise backend REST API for an online bus ticket reservation platform (modeled after services like RedBus). It manages the complete lifecycle of intercity bus travel:
1. **Identity, Authentication & Role Management (`AuthController`)**:
   - Registers users with BCrypt-hashed passwords (work factor 12) and role assignment (`Admin`, `Manager`, `Associate`, `Customer`).
   - Authenticates credentials at `POST /api/Auth/login` and returns a signed JWT Bearer token (valid for 120 minutes) and a 7-day refresh token.
   - Enforces role-based access control across all protected endpoints.
2. **Bus Vendor Management (`BusBookingController`)**:
   - Manages bus operators/fleets (`BusVendors`) including contact details and active status.
3. **City Locations & Boarding/Drop-off Points (`BusBookingController`)**:
   - Maintains city/station stops (`BusLocations` with unique station codes like `NYC`, `BOS`, `DC`) and specific pickup/drop-off landmarks (`BusAddressLocations`) within each city.
   - Caches location lookups in `.NET 10 HybridCache` for 60 minutes.
4. **Schedules & High-Performance Bus Search (`BusBookingController`)**:
   - Bus vendors publish trips (`BusSchedules`) specifying origin, destination, travel date, departure/arrival times, total seats, and ticket price.
   - Customers search available buses (`GET /api/BusBooking/searchBus` and `searchBus2`) using Dapper queries backed by SQL Server covering indexes and `HybridCache` (2-minute TTL), dynamically computing `availableSeats = totalSeats - bookedSeats`.
5. **Concurrency-Safe Ticket Booking & Seat Allocation (`BusBookingController`)**:
   - Customers inspect occupied seats (`GET /api/BusBooking/getBookedSeats`) and book one or more passenger seats (`POST /api/BusBooking/PostBusBooking`).
   - Prevents double-booking under high concurrency using a ** double defense**:
     - Application layer: Executes inside a `SERIALIZABLE` database transaction via EF Core's retry-aware `CreateExecutionStrategy()`, checking seat range and availability before inserting.
     - Database layer: Enforces a `UNIQUE CLUSTERED INDEX` on the schema-bound indexed view `dbo.vw_ActiveScheduleSeats(ScheduleId, SeatNo)`. Even if two requests race at the microsecond level, SQL Server guarantees only one succeeds while the other receives HTTP `409 Conflict` (`SeatAlreadyBookedException`).

---

### 2.2 Clean Architecture Solution Structure
Located at `C:\Projects\BusBookingApi\BusBooking.slnx`:

```text
C:\Projects\BusBookingApi\
├── BusBooking.slnx
├── .gitignore
├── scripts\
│   ├── 01_Initial_Schema_And_Covering_Indexes.sql
│   ├── 02_Seed_Default_Role_Users.sql
│   ├── seed_users_via_api.ps1
│   ├── verify_admin_login.ps1
│   ├── test_auth_and_swagger.ps1
│   ├── test_swagger.ps1
│   └── test_full_system.ps1
└── src\
    ├── BusBooking.Domain\           # Core Entities & Enums (Zero external dependencies)
    ├── BusBooking.Application\      # CQRS (MediatR), FluentValidation, DTOs, Metrics
    ├── BusBooking.Infrastructure\   # EF Core 10 DbContext, Dapper, JWT, HybridCache
    └── BusBooking.Api\              # Controllers, Middleware, Program.cs, Swagger, OTel
```

---

### 2.3 Complete Endpoint & Role-Based Authorization (RBAC) Matrix

#### A. `AuthController` (`/api/Auth` & `/api/v1/Auth`)
| HTTP Method | Endpoint Route | Allowed Roles | Description |
| :--- | :--- | :--- | :--- |
| `POST` | `/api/Auth/login` | `AllowAnonymous` | Authenticates username/password and returns JWT token |
| `POST` | `/api/Auth/AddNewUser` | `AllowAnonymous` | Registers a new user with BCrypt password hashing |
| `GET` | `/api/Auth/GetAllUsers` | `Admin`, `Manager` | Lists all users (passwords masked) |
| `POST` | `/api/Auth/UpdateUser` | Any Authenticated User | Updates user profile / password |
| `DELETE` | `/api/Auth/DeleteUserByUserId?userId={id}` | `Admin` | Deletes a user by ID |

#### B. `BusBookingController` (`/api/BusBooking` & `/api/v1/BusBooking`)
| Category | HTTP Method | Endpoint Route | Allowed Roles |
| :--- | :--- | :--- | :--- |
| **Vendors** | `GET` | `/api/BusBooking/GetBusVendors` | `Admin`, `Manager`, `Associate` |
| **Vendors** | `GET` | `/api/BusBooking/GetBusVendorsById?id={id}` | `Admin`, `Manager`, `Associate` |
| **Vendors** | `POST` | `/api/BusBooking/PostBusVendor` | `Admin`, `Manager` |
| **Vendors** | `PUT` | `/api/BusBooking/PutBusVendors?id={id}` | `Admin`, `Manager` |
| **Vendors** | `DELETE` | `/api/BusBooking/DeleteBusVendor?id={id}` | `Admin` |
| **Locations** | `GET` | `/api/BusBooking/GetBusLocations` | `AllowAnonymous` |
| **Locations** | `GET` | `/api/BusBooking/GetBusLocationById?id={id}` | `AllowAnonymous` |
| **Locations** | `POST` | `/api/BusBooking/PostBusLocation` | `Admin`, `Manager` |
| **Locations** | `PUT` | `/api/BusBooking/PutBusLocation?id={id}` | `Admin`, `Manager` |
| **Locations** | `DELETE` | `/api/BusBooking/DeleteBusLocation?id={id}` | `Admin` |
| **Locations** | `GET` | `/api/BusBooking/getAddressByLocationId?locationid={id}` | `AllowAnonymous` |
| **Locations** | `POST` | `/api/BusBooking/PostBusLocationAddress` | `Admin`, `Manager` |
| **Schedules & Search** | `GET` | `/api/BusBooking/GetAvailableRoutes` | `AllowAnonymous` |
| **Schedules & Search** | `GET` | `/api/BusBooking/GetBusSchedules?vendorId={id}` | `Admin`, `Manager`, `Associate` |
| **Schedules & Search** | `GET` | `/api/BusBooking/GetBusScheduleById?id={id}` | `Admin`, `Manager`, `Associate` |
| **Schedules & Search** | `POST` | `/api/BusBooking/PostBusSchedule` | `Admin`, `Manager` |
| **Schedules & Search** | `PUT` | `/api/BusBooking/PutBusSchedule?id={id}` | `Admin`, `Manager` |
| **Schedules & Search** | `DELETE` | `/api/BusBooking/DeleteBusSchedule?id={id}` | `Admin` |
| **Schedules & Search** | `GET` | `/api/BusBooking/searchBus` | `AllowAnonymous` (Rate-limited: `sliding-search`) |
| **Schedules & Search** | `GET` | `/api/BusBooking/searchBus2` | `AllowAnonymous` (Rate-limited: `sliding-search`) |
| **Bookings** | `GET` | `/api/BusBooking/getBookedSeats?shceduleId={id}` | `AllowAnonymous` |
| **Bookings** | `GET` | `/api/BusBooking/GetAllBusBookings?vendorId={id}` | `Admin`, `Manager`, `Associate` |
| **Bookings** | `GET` | `/api/BusBooking/GetBusBooking?id={id}` | Any Authenticated User |
| **Bookings** | `POST` | `/api/BusBooking/PostBusBooking` | Any Authenticated User (Rate-limited: `token-booking`) |
| **Bookings** | `DELETE` | `/api/BusBooking/DeleteBusBooking?id={id}` | Any Authenticated User |

---

## SECTION 3: STEP-BY-STEP INSTRUCTIONS TO RUN THE APPLICATION LOCALLY

### 3.1 Prerequisites
Ensure the following tools are installed on your Windows development machine:
1. **.NET 10 SDK** (`10.0.100` or later) — Verify via `dotnet --version`.
2. **Microsoft SQL Server** (Developer, Express, or Enterprise Edition running locally on `localhost` / default instance `MSSQLSERVER`) and `sqlcmd` CLI.
3. **PowerShell 5.1+** or **PowerShell 7+** and **Git**.
4. **IDE**: Visual Studio 2022/2026, VS Code (with C# Dev Kit), or JetBrains Rider.

---

### 3.2 Step 1 — Navigate to the Project Directory
Open PowerShell and navigate to the permanent workspace:
```powershell
cd C:\Projects\BusBookingApi
```

---

### 3.3 Step 2 — Verify Configuration (`appsettings.json`)
Inspect `C:\Projects\BusBookingApi\src\BusBooking.Api\appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=BusBookingDb;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "JwtSettings": {
    "Secret": "SuperSecretKeyForBusBookingApiMustBeAtLeast32BytesLong!",
    "Issuer": "BusBookingApi",
    "Audience": "BusBookingClient",
    "ExpiryMinutes": 120
  }
}
```
> If you use a named SQL Server instance (e.g., `localhost\SQLEXPRESS`), update `Server=localhost` in `DefaultConnection` accordingly.

---

### 3.4 Step 3 — Create & Seed the SQL Server Database
Run the SQL schema and covering index script, or apply EF Core migrations:

```powershell
# Option A: Apply via SQL script (creates BusBookingDb, all 7 tables, covering indexes, indexed view, and baseline seed data)
sqlcmd -S localhost -E -i "C:\Projects\BusBookingApi\scripts\01_Initial_Schema_And_Covering_Indexes.sql"

# Option B: Apply via EF Core Migrations
dotnet ef database update --project "src\BusBooking.Infrastructure\BusBooking.Infrastructure.csproj" --startup-project "src\BusBooking.Api\BusBooking.Api.csproj"
```

---

### 3.5 Step 4 — Build the Solution
Compile all four projects and verify zero warnings or errors:
```powershell
dotnet build "C:\Projects\BusBookingApi\BusBooking.slnx"
```

---

### 3.6 Step 5 — Run the Web API
Start the API server on `http://localhost:5241`:
```powershell
dotnet run --project "C:\Projects\BusBookingApi\src\BusBooking.Api\BusBooking.Api.csproj" --urls "http://localhost:5241"
```

If this is a fresh database and default role users haven't been seeded yet, you can run the seeding script in a separate PowerShell window:
```powershell
powershell -ExecutionPolicy Bypass -File "C:\Projects\BusBookingApi\scripts\seed_users_via_api.ps1"
```

---

### 3.7 Step 6 — Explore & Test via Swagger UI
1. Open your browser and navigate to:
   - **Swagger UI**: `http://localhost:5241/swagger` (or simply `http://localhost:5241/`)
   - **Liveness Health Probe**: `http://localhost:5241/health/live`
   - **Readiness Health Probe (SQL Server)**: `http://localhost:5241/health/ready`

2. **Default Seeded Role Accounts**:

| Role | Username | Password | Email | Access Level |
| :--- | :--- | :--- | :--- | :--- |
| **Admin** | `admin` | `Admin@123` | `admin@busbooking.local` | Full access (including `DELETE` operations) |
| **Manager** | `manager` | `Manager@123` | `manager@busbooking.local` | Create/Update/Read vendors, locations, schedules, users |
| **Associate** | `associate` | `Associate@123` | `associate@busbooking.local` | Read vendors, schedules, bookings; create bookings |

3. **How to Authorize in Swagger UI**:
   - Expand **`POST /api/Auth/login`** -> Click **Try it out**.
   - Enter:
     ```json
     {
       "userName": "admin",
       "password": "Admin@123"
     }
     ```
   - Click **Execute** and copy the `token` string from `data.token` in the response body.
   - Scroll to the top of Swagger UI and click the **`Authorize`** button.
   - Paste your token into the **Value** field (works both with raw `<token>` or `Bearer <token>`) -> Click **Authorize** -> Click **Close**.
   - Now test **`GET /api/Auth/GetAllUsers`** or any protected endpoint in `BusBookingController`.

---

### 3.8 Step 7 — Run the Automated End-to-End Verification Scripts
Make sure no manual instance of `BusBooking.Api` is occupying port `5241`, then run:

```powershell
# Full Authentication, RBAC & Swagger verification suite
powershell -ExecutionPolicy Bypass -File "C:\Projects\BusBookingApi\scripts\test_auth_and_swagger.ps1"

# Quick Admin Login probe
powershell -ExecutionPolicy Bypass -File "C:\Projects\BusBookingApi\scripts\verify_admin_login.ps1"

# Health checks + Locations probe
powershell -ExecutionPolicy Bypass -File "C:\Projects\BusBookingApi\scripts\test_full_system.ps1"
```

---

### 3.9 Troubleshooting Tips for New Engineers
1. **Build Error `File is locked by BusBooking.Api.exe`**:
   Stop any running instance before building:
   ```powershell
   Get-Process -Name "BusBooking.Api" -ErrorAction SilentlyContinue | Stop-Process -Force
   ```
2. **PowerShell 5.1 `Invoke-WebRequest` Hanging**:
   Always include `-UseBasicParsing` when calling `Invoke-WebRequest` in Windows PowerShell 5.1 so it does not invoke the legacy Internet Explorer MSHTML DOM parser.
3. **Swagger UI Returning `401 Unauthorized` After Code Changes**:
   Hard-refresh the browser (`Ctrl+F5`) after restarting the API so Swagger UI reloads `/swagger/v1/swagger.json`.
```
