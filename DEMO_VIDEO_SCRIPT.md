# Part 3 Demo Video Script - GLMS (SOA, Docker & Automated Testing)

**Target length:** 6-9 minutes. Speak naturally; the lecturer marks on your explanation of the flow, not polish.

**Before you hit record:**
- Run `docker compose up --build` in the GLMS-SOA folder and wait for all 3 containers to be green in Docker Desktop.
- Have these tabs/windows ready: Docker Desktop, Visual Studio (solution open), browser at localhost:5080/swagger and localhost:5090, and the GitHub repo page.
- Demo login: **admin / Admin@123**

---

## 1. Intro (about 30 sec)
"Hi, I'm Lesego Masike, student ST10058999. This is my Part 3 submission for PROG7311 - the Green Logistics Management System. In Part 2 this was a single MVC app that talked straight to the database. For Part 3 I've split it into a Service-Oriented Architecture: a separate Web API that owns the data, an MVC client that only calls that API over HTTP, JWT login, automated tests, and the whole thing runs in Docker. Let me show you."

## 2. The solution structure in Visual Studio (about 1 min)
- Show the Solution Explorer with the three projects.
- "There are three projects: GreenLogisticsMS.Api is the back end - it's the only one with the EF Core DbContext and the only one that touches SQL Server. GreenLogisticsMS.Web is the front end - notice it has no database packages, it just calls the API. And GreenLogisticsMS.Tests holds my unit and integration tests."
- Open `ApiClient.cs` in the Web project briefly: "This is the typed HttpClient the MVC controllers use instead of a database - it sends requests to the API and attaches the JWT token."

## 3. Docker - the containers running (about 1 min) [KEY for the brief]
- Switch to Docker Desktop, Containers view.
- "Here's the whole system running as containers from one docker compose up. There are three: sql-server-db on port 1433, glms-backend-api on 5080, and glms-frontend-web on 5090 - all green and running on an internal Docker network so they find each other by name."
- Open the backend-api container's **Logs** tab, scroll to show "Database migrated successfully".
- "You can see the API connected to SQL Server and ran my EF Core migration automatically on startup - that fixes the missing-migration feedback from Part 2."

## 4. The Web API via Swagger (about 1.5 min)
- Browser to `http://localhost:5080/swagger`.
- "The API is self-documenting through Swagger. You can see the endpoints: Auth for login, and full CRUD for Clients, Contracts and Service Requests, including the PATCH /api/contracts/{id}/status endpoint for the approval workflow."
- Expand `POST /api/auth/login`, Try it out, body `{ "username": "admin", "password": "Admin@123" }`, Execute. Show the 200 and the JWT token.
- "That returns a JWT. If I try a protected endpoint without it I get a 401." (Optionally expand GET /api/contracts and Execute without auth to show 401, then Authorize with the token and show it works.)

## 5. The MVC web client (about 2 min)
- Browser to `http://localhost:5090`.
- "This is the MVC front end. It has no database connection at all - everything you see comes from the API."
- Log in with admin / Admin@123. "The login posts to the API, gets the JWT back, and stores it in session."
- Go to **Clients** - "This list is fetched live from GET /api/clients."
- Create or open a **Contract**, show the status change. "Changing the status here calls the PATCH endpoint on the API."
- Create a **Service Request** - "When I save this, the API converts the USD cost to ZAR using the live exchange rate, and it blocks the request if the contract is expired - that business rule lives in the API, not here."

## 6. Automated tests (about 1 min)
- Back in Visual Studio, open Test Explorer (or show the integration test file).
- "These integration tests start the real API in memory and call the actual endpoints - they check GET /api/contracts returns 200 with JSON, that an unauthenticated call returns 401, and that a service request on an expired contract returns 400. In a CI/CD pipeline these run on every push to catch breaking changes before deployment."
- If quick, run the tests and show them pass (green).

## 7. GitHub (about 30 sec)
- Show the repo page: EMGPMD/final-poe-ST10058999-Lesego-Masike.
- "The full solution is on GitHub - both projects, the Dockerfiles, docker-compose.yml and the reflection report."

## 8. Close (about 20 sec)
"So that's Part 3: the system is now a decoupled API and client, secured with JWT, tested automatically, and fully containerised with Docker Compose. Thanks for watching."

---

### Quick tips
- If a container isn't green, click its Start (play) button and wait, then start recording.
- Keep the cursor moving slowly and pause on each screen for a second so it's readable.
- You don't need to read this word-for-word - the bullet points under each section are the things to actually show.
