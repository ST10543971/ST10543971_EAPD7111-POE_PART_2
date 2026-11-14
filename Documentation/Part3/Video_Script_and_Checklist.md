# Part 3 Video Script and Submission Checklist

**Target duration:** 10–15 minutes  
**Student:** [Your name]  
**Student number:** ST10543971  
**Repository:** https://github.com/ST10543971/ST10543971_EAPD7111-POE_PART_2

This is a rehearsal guide. Speak in your own words, show the actual result on screen, and do not claim a command succeeded unless the result is visible. Say each important filename and container name aloud while showing it.

## Before recording

- [ ] Confirm the GitHub repository link works while logged out, or confirm lecturer access.
- [ ] Start Docker Desktop with the Linux engine enabled.
- [ ] Copy `.env.example` to `.env`; do not show the secret values on screen.
- [ ] Run `docker compose up --build -d` from the solution root.
- [ ] Capture current screenshots of Containers, Images, Volumes, Swagger, and the frontend.
- [ ] Run `dotnet test TechMove.Tests\TechMove.Tests.csproj` and capture the passing result.
- [ ] Open the successful GitHub Actions run for the commit being demonstrated.
- [ ] Close unrelated applications, browser developer tools, notifications, and personal information.
- [ ] Set the editor zoom to approximately 125% and use a readable terminal font.
- [ ] Rehearse once and keep the recording within 10–15 minutes.

## Timed walkthrough

### 0:00–1:00 — Repository and architecture

**Show:** GitHub repository, solution tree, and the three projects: `TechMove`, `TechMove.Api`, and `TechMove.Tests`.

**Say:**

> “This is the TechMove logistics management system. Part 3 separates the frontend from the backend API. `TechMove` is the MVC presentation project, `TechMove.Api` owns the database and business services, and `TechMove.Tests` contains unit and API integration tests. The browser communicates with the frontend, and the frontend uses typed HTTP clients to call the API.”

**Point out:** `TechMove.Api/Controllers`, `Services`, `Repositories`, `Data`, and `TechMove/Services/ApiClients`.

### 1:00–2:30 — Docker stack running

**Show:** Terminal output and Docker Desktop Containers tab.

**Run or show:**

```powershell
docker compose ps
docker compose logs --tail=80 techmove-api
```

**Say:**

> “The Compose stack contains three services: `sql-server-db`, `techmove-api`, and `techmove-web`. They are attached to `techmove-network`. SQL Server has a healthcheck, so the API waits for a healthy database. The API stores uploaded agreements in the named `agreements-data` volume, while SQL Server uses `sql-data`.”

Do not show `.env` values. If the stack was not actually run, say that this is a configuration review rather than runtime evidence.

### 2:30–3:30 — Compose configuration

**Show:** `docker-compose.yml`.

**Say:**

> “The API is reached from the web container using `http://techmove-api:8080`. This is the Compose service name; `localhost` would point back to the web container and would be incorrect. The host mappings are port 5000 for the API and port 5001 for the frontend. Configuration such as the connection string, JWT key, seed passwords, and file-storage path is passed through environment variables rather than hard-coded into the Compose file.”

**Point out:** `depends_on`, SQL healthcheck, `techmove-network`, `sql-data`, `agreements-data`, and `ApiSettings__BaseUrl`.

### 3:30–4:30 — Multi-stage Dockerfiles

**Show:** `TechMove.Api/Dockerfile` and root `Dockerfile`.

**Say:**

> “Both images use a build stage based on the .NET 10 SDK and a smaller ASP.NET runtime stage. The project file is copied and restored before the rest of the source, which improves layer caching. The runtime stage does not include the SDK. Both containers listen on port 8080 and use a .NET DLL entrypoint. The API creates `/app/Uploads` and assigns it to the non-root `app` user before switching users.”

**Point out:** `COPY ...csproj`, `dotnet restore`, `dotnet publish --no-restore`, runtime image, `USER app`, `EXPOSE 8080`, and each entrypoint.

### 4:30–6:00 — Swagger and API behaviour

**Show:** `http://localhost:5000/swagger`.

**Demonstrate if the stack is running:**

1. Authenticate through the login endpoint and copy the JWT.
2. Use Swagger’s **Authorize** control with `Bearer <token>`.
3. Show a GET endpoint.
4. Show a POST that returns `201 Created`.
5. Show validation or business-rule failure returning `400`.
6. Show a missing resource returning `404`.
7. Show an unauthenticated request returning `401`.

**Say:**

> “The API uses explicit controller routes and HTTP verbs. Controllers delegate to services rather than containing persistence queries. Responses use DTOs, and status codes communicate success, validation, authentication, and not-found outcomes.”

Only demonstrate status codes that are visible in the actual response.

### 6:00–7:00 — API layering and dependency injection

**Show:** one controller, its service, its repository interface/implementation, and `TechMove.Api/Program.cs`.

Recommended path:

- `TechMove.Api/Controllers/ServiceRequestsController.cs`
- `TechMove.Api/Services/ServiceRequestService.cs`
- `TechMove.Api/Repositories/IServiceRequestRepository.cs`
- `TechMove.Api/Repositories/ServiceRequestRepository.cs`
- `TechMove.Api/Program.cs`

**Say:**

> “The request follows Controller to Service to Repository to DbContext. The controller handles HTTP concerns, the service enforces business rules, the repository handles persistence, and the DbContext represents EF Core database access. Interfaces are registered in `Program.cs`, allowing the tests to replace external services such as currency conversion.”

### 7:00–8:15 — Design patterns and problem solving

**Show and explain:**

- **Repository pattern:** `IClientRepository`, `IContractRepository`, and `IServiceRequestRepository` isolate EF Core persistence from business logic.
- **Service layer pattern:** `ContractService` and `ServiceRequestService` centralize validation, file handling, and contract rules.
- **Dependency injection:** `Program.cs` supplies repositories, services, storage, and the currency HTTP client, making components replaceable and testable.

**Say:**

> “These patterns are useful here because the API has different responsibilities: database access, business rules, file storage, and external exchange-rate access. Separating them makes the controller smaller and gives the integration tests controlled substitutes.”

### 8:15–10:45 — Frontend workflow

**Show:** the browser at `http://localhost:5001`.

Demonstrate the available flow, using test data and safe credentials:

1. Login.
2. Create or view a client.
3. Create a contract and upload a PDF if the scenario requires it.
4. View or download the agreement.
5. Create a service request for an active contract.
6. Show the USD-to-ZAR result if available.
7. Attempt a request for an expired or on-hold contract and show the rejection.
8. Show role differences for admin and manager only if those accounts and permissions are configured.

**Say:**

> “The browser is using the MVC frontend. The frontend does not access EF Core or SQL Server. Its typed API client services call the backend, and the JWT handler forwards the authenticated token. API failures are converted into a user-facing message rather than exposing internal exception details.”

Do not expose real passwords or personal data.

### 10:45–12:00 — Frontend code and failure handling

**Show:**

- `Services/ApiClients/ApiClients.cs`
- `Program.cs`
- `Controllers/AccountController.cs`
- One API-backed controller, such as `Controllers/ContractsController.cs`

**Say:**

> “The frontend registers typed HTTP clients with a configured base URL. The base URL is `http://techmove-api:8080` in Compose. Cookie authentication is used for the browser session, while the JWT bearer handler forwards the API token. The frontend project has no application DbContext or SQL connection string.”

Optionally stop the API only if you have rehearsed recovery:

```powershell
docker compose stop techmove-api
```

Refresh the frontend and show the friendly unavailable message, then restore it:

```powershell
docker compose start techmove-api
```

### 12:00–13:30 — Automated tests

**Show:** Visual Studio Test Explorer and the terminal.

**Run:**

```powershell
dotnet test TechMove.Tests\TechMove.Tests.csproj --logger "trx;LogFileName=video-tests.trx"
```

**Show or explain:**

- `WorkflowTests.ExpiredContractCannotCreateRequest`
- `AgreementsServiceRatesTests.CreateThenGet_ServiceRequest_ReturnsSameData`
- `AgreementsServiceRatesTests.CreateServiceRequest_ExpiredOrOnHoldContract_Returns400`
- `AgreementsServiceRatesTests.CreateServiceRequest_ActiveContract_Returns201AndZarCostEquals_UsdTimes18_50`
- `AuthAndContractsTests.Login_WithWrongPassword_Returns401`

**Say:**

> “The integration tests use `WebApplicationFactory`, an in-memory SQLite database, temporary file storage, and a deterministic currency service. This keeps the test run repeatable and avoids requiring the Docker SQL Server or an external exchange-rate API. The tests cover authentication, create-and-read behaviour, business rules, and currency conversion.”

Read the actual final count from the test output rather than using a memorised number.

### 13:30–14:30 — GitHub Actions

**Show:** the successful Actions run and `.github/workflows/ci.yml`.

**Say:**

> “The workflow runs on pushes and pull requests to `main`. It restores, builds, and tests the solution using .NET 10. Test results are uploaded as an artifact. After the test job succeeds, the Docker build job builds the Compose services. This makes the test result a gate before image building. This workflow builds images; it does not deploy them to production.”

### 14:30–15:00 — Wrap-up

**Say:**

> “The main Part 3 improvements are the API boundary, automated tests, and reproducible container configuration. The API owns persistence and file storage, the frontend communicates through typed HTTP clients, and Compose supplies service networking and environment-specific configuration. The remaining production considerations would include secret management, backups, image scanning, observability, and an orchestration platform beyond Compose.”

## Evidence and submission checklist

### GitHub

- [ ] Repository URL works while logged out or lecturer access is configured.
- [ ] Full solution is present, including `TechMove.Api`, `TechMove`, and `TechMove.Tests`.
- [ ] `TechMove.Api/Dockerfile`, root `Dockerfile`, `docker-compose.yml`, `.env.example`, `.dockerignore`, `.github/workflows/ci.yml`, and `README.md` are present.
- [ ] `.env` is not tracked; no real passwords or JWT keys are committed.
- [ ] `bin/`, `obj/`, `.vs/`, and other local files are ignored.
- [ ] README includes local run, Docker run, test commands, endpoints, and seeded-login guidance if applicable.

### Runtime screenshots

Save in `docs/screenshots/` using descriptive filenames, for example:

- `docker-containers.png`
- `docker-images.png`
- `docker-volumes.png`
- `swagger-api.png`
- `web-frontend.png`
- `passing-tests.png`
- `github-actions.png`

Check each screenshot for visible secrets, tokens, passwords, private paths, or unrelated personal information.

### Reflection PDF

- [ ] Complete `Reflection_Report_Template.md` in your own words.
- [ ] Verify all Harvard sources and access dates.
- [ ] Ensure every in-text citation has a matching reference-list entry.
- [ ] Insert only evidence you personally captured or can substantiate.
- [ ] Export as `ST10543971_Part3_Reflection.pdf`.

### Video

- [ ] Voice is audible throughout.
- [ ] File names and container names are spoken clearly.
- [ ] Browser and editor text are readable.
- [ ] No secrets or personal information are visible.
- [ ] Runtime claims match the captured terminal output.
- [ ] The final video link/file is tested in a private window.
- [ ] Upload location and naming follow the lecturer/ARC instructions.
