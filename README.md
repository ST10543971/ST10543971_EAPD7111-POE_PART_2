# TechMove GLMS

[![CI](https://github.com/ST10543971/ST10543971_EAPD7111-POE_PART_2/actions/workflows/ci.yml/badge.svg)](https://github.com/ST10543971/ST10543971_EAPD7111-POE_PART_2/actions/workflows/ci.yml)

TechMove is a .NET 10 logistics management system with an API backend, MVC frontend, and xUnit unit/integration tests.

## Tests

```powershell
dotnet test TechMove.Tests\TechMove.Tests.csproj
```

The integration tests use an in-memory SQLite database, a deterministic exchange-rate fake, and temporary file storage. They do not require SQL Server or internet access.

## Part 3 submission materials

- [Technical reflection report template](Documentation/Part3/Reflection_Report_Template.md)
- [Video script and submission checklist](Documentation/Part3/Video_Script_and_Checklist.md)

## Docker Compose

Docker Desktop must be running with the Linux container engine enabled. Copy the environment template and start the three-container stack from the solution root:

```powershell
Copy-Item .env.example .env
docker compose up --build -d
docker compose ps
docker compose logs -f techmove-api
```

The local endpoints are:

- Frontend: <http://localhost:5001>
- API Swagger: <http://localhost:5000/swagger>
- API health: <http://localhost:5000/health>

The frontend reaches the API through the Compose service name, not `localhost`:

```powershell
docker exec techmove-web getent hosts techmove-api
```

If `getent` is unavailable in the frontend image, inspect the configured URL and network membership:

```powershell
docker compose config
docker inspect techmove-web
docker network inspect techmove-network
```

Confirm that the API initialized the database and applied migrations by reviewing its startup logs:

```powershell
docker compose logs techmove-api
```

The SQL Server and uploaded agreement data use named volumes. Verify uploaded files survive an API restart:

```powershell
docker compose exec techmove-api ls -la /app/Uploads
docker compose restart techmove-api
docker compose exec techmove-api ls -la /app/Uploads
```

Verify database and file data survive stopping and recreating containers:

```powershell
docker compose down
docker compose up -d
docker compose ps
```

To deliberately remove the named volumes and all persisted data, use:

```powershell
docker compose down -v
```

Common fixes:

- `Login failed for user 'sa'`: ensure `.env` has the same password used to initialize the `sql-data` volume, or use `docker compose down -v` and start again.
- SQL Server exits immediately: use a password containing uppercase, lowercase, a number, and a symbol.
- Frontend reports the API is unavailable: confirm `ApiSettings__BaseUrl` is `http://techmove-api:8080` inside Compose.
- PDF upload permission errors: rebuild the API image so `/app/Uploads` is owned by the non-root `app` user.
- Port conflicts: stop local applications using ports 1433, 5000, or 5001, or change the host-side port mapping.

### Docker evidence checklist

Capture the following evidence in `docs/screenshots/` after the stack is running:

- Docker Desktop Containers tab showing `sql-server-db`, `techmove-api`, and `techmove-web` running.
- Docker Desktop Images tab showing the built API and frontend images.
- Docker Desktop Volumes tab showing `sql-data` and `agreements-data`.
- Swagger running at `http://localhost:5000/swagger`.
- The frontend running at `http://localhost:5001`.

Do not commit `.env`, passwords, uploaded documents, build output, or local Docker screenshots containing secrets.
