# Green Logistics Management System (GLMS) — Part 3 (SOA, Docker & Automated Testing)

PROG7311 Programming 3A — Portfolio of Evidence, Part 3.

This solution refactors the Part 2 monolithic MVC prototype into a **Service-Oriented Architecture**: a stand-alone **Web API** owns the database, and a thin **MVC client** consumes that API over HTTP. The whole stack is containerised with Docker Compose and covered by automated **integration tests**.

---

## Solution structure

```
GLMS-SOA/
├── GreenLogisticsMS.sln
├── docker-compose.yml              # 3 containers: db, api, web
├── GreenLogisticsMS.Api/           # ASP.NET Core Web API (talks to SQL Server)
│   ├── Controllers/                # Auth, Clients, Contracts, ServiceRequests
│   ├── Data/ApplicationDbContext.cs
│   ├── DTOs/                       # JSON contract (decoupled from entities)
│   ├── Migrations/                 # EF Core migration (addresses Part 2 feedback)
│   ├── Models/                     # Client, Contract, ServiceRequest, AppUser
│   ├── Services/                   # Currency, File, Auth (JWT) + design patterns
│   └── Dockerfile
├── GreenLogisticsMS.Web/           # ASP.NET Core MVC client (NO database access)
│   ├── Controllers/                # Account (login), Clients, Contracts, ...
│   ├── Services/ApiClient.cs       # typed HttpClient -> Web API, attaches JWT
│   ├── Views/
│   └── Dockerfile
└── GreenLogisticsMS.Tests/
    ├── Integration/                # WebApplicationFactory tests hitting the API
    └── Unit/                       # currency + design-pattern unit tests
```

## What changed since Part 2 (and why)

| Part 2 feedback | Fix in Part 3 |
|---|---|
| *"No migration script provided"* (DB 5/10) | A full EF Core migration (`Migrations/`) is committed and applied automatically on API startup. |
| *"No logging in to the system"* (Workflow 7/15) | JWT bearer authentication. The API rejects unauthenticated calls (401); the MVC client has a login screen and stores the token in session. |
| *"Controllers call DbContext directly / tightly coupled"* | Business logic and data access now live behind the Web API. MVC controllers contain **no** EF Core code — they call `IApiClient`. |
| *"Workflow logic has bugs"* | Status workflow is exposed via `PATCH /api/contracts/{id}/status` with server-side rules (e.g. an Expired contract cannot return to Draft; requests are blocked on Expired/OnHold contracts). |

## API endpoints (Swagger-documented)

| Method | Route | Purpose |
|---|---|---|
| POST | `/api/auth/login` | Authenticate, returns a JWT |
| GET | `/api/contracts` | List contracts (filter: `status`, `clientId`, `startFrom`, `startTo`) |
| GET | `/api/contracts/{id}` | Single contract |
| POST | `/api/contracts` | Create a contract |
| PATCH | `/api/contracts/{id}/status` | Approve / decline / hold (workflow) |
| POST/GET | `/api/contracts/{id}/agreement` | Upload / download signed PDF |
| GET/POST/PUT/DELETE | `/api/clients` | Client CRUD |
| GET/POST/DELETE | `/api/servicerequests` | Service requests (USD→ZAR conversion + Factory) |
| GET | `/health` | Liveness probe (anonymous) |

All endpoints except `login` and `health` require `Authorization: Bearer <token>`.

**Demo account:** `admin` / `Admin@123`

---

## Running with Docker (recommended)

> Requires Docker Desktop.

```bash
cd GLMS-SOA
docker compose up --build
```

This starts three containers:

| Container | Image / build | Host port |
|---|---|---|
| `glms-sql-server-db` | mssql/server:2022 | 1433 |
| `glms-backend-api` | builds `GreenLogisticsMS.Api/Dockerfile` | 5080 |
| `glms-frontend-web` | builds `GreenLogisticsMS.Web/Dockerfile` | 5090 |

Then browse to:

- **MVC client:** http://localhost:5090
- **Swagger UI:** http://localhost:5080/swagger

The API waits for SQL Server to become healthy, then applies the EF Core migration automatically (with retry).

To stop and remove everything:

```bash
docker compose down            # keep data
docker compose down -v         # also remove the SQL volume
```

## Running locally (without Docker)

1. Start SQL Server LocalDB (or update the connection string in `GreenLogisticsMS.Api/appsettings.Development.json`).
2. API: `dotnet run --project GreenLogisticsMS.Api` → https://localhost:5080/swagger
3. Web: `dotnet run --project GreenLogisticsMS.Web` → http://localhost:5090

The API auto-migrates on first run.

## Running the tests

```bash
dotnet test
```

The integration tests use `WebApplicationFactory<Program>` with an EF Core **in-memory** database, so they need no SQL Server and run anywhere (including a CI pipeline). They start the API in-process and assert real HTTP responses — e.g. `GET /api/contracts` returns **200 OK** with non-null JSON, unauthenticated calls return **401**, and a service request on an expired contract returns **400**.

---

## Tech stack

- .NET 8 / ASP.NET Core (Web API + MVC)
- Entity Framework Core 8 (SQL Server)
- JWT bearer authentication
- Swashbuckle (Swagger / OpenAPI)
- xUnit + Moq + Microsoft.AspNetCore.Mvc.Testing
- Docker & Docker Compose

## Submission checklist (per brief)

- [x] Two projects: MVC client + Web API, in one solution
- [x] API exposes GET/POST/PATCH endpoints; MVC uses HttpClient only
- [x] Swagger / OpenAPI enabled
- [x] Authentication included (JWT)
- [x] Automated API integration tests
- [x] Dockerfile for API and for Web
- [x] `docker-compose.yml` (db + api + web on internal network)
- [ ] Push full solution to GitHub *(do this — missing GitHub = 5% penalty)*
- [ ] Screenshots of the system running via Docker Desktop
- [ ] Technical Reflection Report (PDF) — see `Technical_Reflection_Report`
- [ ] Demo video walking through the app flow
