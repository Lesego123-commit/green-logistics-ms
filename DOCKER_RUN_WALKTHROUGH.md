# Part 3 — Running the GLMS containers (for your Docker Desktop screenshots)

Follow these steps on your own machine. Once the containers are running, ask Claude to take the screenshots.

## Step 1 — Install Docker Desktop
1. Download "Docker Desktop for Windows": https://www.docker.com/products/docker-desktop/
2. Run the installer. Click **Yes** on the admin (UAC) prompt. Keep **"Use WSL 2 instead of Hyper-V"** ticked.
3. **Restart your PC** if asked.
4. Launch **Docker Desktop**. Wait until the bottom-left shows **"Engine running"** (green). You can skip signing in.

## Step 2 — Open a terminal in the solution folder
1. In File Explorer, go to: `D:\prog 7311 assignment\part 3\GLMS-SOA`
2. Make sure you can see `docker-compose.yml`.
3. Click the address bar, type `cmd`, press **Enter** (opens Command Prompt in that folder).

## Step 3 — Build and start everything
Run:

```
docker compose up --build
```

First run takes several minutes (downloads images + builds both apps). Leave the window open.

## Step 4 — Confirm it's ready
Wait for log lines similar to:
- `glms-backend-api  | ... Now listening on: http://[::]:8080`
- `glms-backend-api  | ... Database migrated successfully.`
- `glms-frontend-web | ... Now listening on: http://[::]:8080`

Then open in your browser:
- MVC app:  http://localhost:5090
- Swagger:  http://localhost:5080/swagger
- Login:    admin  /  Admin@123

## Step 5 — Get the screenshots
With the three containers running (sql-server-db, glms-backend-api, glms-frontend-web), tell Claude.
Claude will capture:
- Docker Desktop > Containers view (all three running)
- The running MVC app at localhost:5090
- The Swagger API docs at localhost:5080/swagger

## Stopping
- Press `Ctrl + C` in the terminal, then run `docker compose down`.
- To also wipe the database volume: `docker compose down -v`.

## Troubleshooting
- **"docker: command not found"** — Docker Desktop not fully started; wait for green "Engine running".
- **Port already in use (5080/5090/1433)** — ask Claude to change the ports in docker-compose.yml.
- **SQL container restarting** — give it 1-2 minutes; the API retries the DB connection automatically.
