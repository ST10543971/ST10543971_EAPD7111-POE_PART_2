# Copilot instructions — TechMove GLMS Part 3

## Project context

- This repository is the TechMove Logistics / Global Logistics Management System project.
- The current application is an ASP.NET Core MVC web application in `TechMove.csproj`, targeting `.NET 10`.
- Existing MVC code is organized under `Controllers/`, `Models/`, `Data/`, `Services/`, `Views/`, and `wwwroot/`.
- Existing automated tests are in `TechMove.Tests/` and use xUnit.
- Inspect the current solution and project files before making assumptions. Preserve existing behavior and migrations unless the requested change requires otherwise.

## Target Part 3 architecture

When implementing the Part 3 modernization, evolve the application toward:

```text
Browser -> TechMove.Web (MVC, cookie authentication, API client) -> TechMove.Api (JWT, services, repositories, EF Core) -> SQL Server
																			-> external exchange-rate API
```

- The API must be the only component that accesses SQL Server, uploaded files, and the external exchange-rate API.
- The MVC front end must communicate with the API through typed `HttpClient` services.
- The MVC project must not reference EF Core, `DbContext`, repositories, or database connection strings after decoupling.
- Do not introduce Blazor or Razor Pages unless explicitly requested; the existing presentation layer is MVC.

## Required engineering rules

1. Inspect existing code, project references, migrations, patterns, services, and tests before generating or moving code.
2. Reuse and preserve the existing Part 2 domain behavior and design patterns. Do not rewrite or delete working functionality without a clear reason.
3. Keep the API layering as `Controller -> Service -> Repository -> DbContext`.
4. Controllers must not contain EF Core queries, LINQ persistence logic, or business rules.
5. Use constructor dependency injection and interfaces for services and repositories. Register every implementation in `Program.cs`.
6. Use `async`/`await` and `CancellationToken` for I/O-bound operations. Never use `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`.
7. API controllers must use `[ApiController]`, explicit route attributes, and explicit HTTP verb attributes.
8. API endpoints must return DTOs rather than EF Core entities and must use appropriate status codes, including `201 CreatedAtAction`, `204`, `400`, `401`, `403`, `404`, and `409` where applicable.
9. Keep secrets out of source control. Use configuration placeholders and environment variables for connection strings, JWT settings, API base URLs, and database credentials. Never commit `.env`.
10. Preserve the business rules that a service request cannot be created for an expired or on-hold contract, only PDF agreement uploads are accepted, uploaded files use GUID-based names, and currency calculations use `decimal`.
11. MVC API clients must handle timeouts, API availability failures, and non-success responses gracefully. Log failures and show a useful user-facing message without exposing secrets or internal exception details.
12. Add unit tests for new public business behavior and integration tests for new API endpoints, including a success case and at least one failure case.
13. Use the existing test framework and conventions unless there is a compelling repository-wide reason to change them.
14. Make focused changes. Do not perform unrelated cleanup, package upgrades, or formatting changes.
15. After each implementation step, run `dotnet build` and `dotnet test`. Address errors caused by the change before continuing.
16. Explain changes briefly in plain language so they can be demonstrated and defended in the assessment video.
17. Add comments only when they explain non-obvious intent; do not comment obvious code.

## Docker and configuration rules

- Use multi-stage Dockerfiles appropriate for `.NET 10`.
- Docker Compose services must communicate using Compose service names, not `localhost` between containers.
- Pass environment-specific values through environment variables or Compose configuration.
- Do not commit real passwords, JWT keys, connection strings, uploaded documents, build output, or local IDE state.

## Change checklist

Before considering a change complete:

- Existing behavior and migrations remain intact.
- Project references still enforce the intended layer boundaries.
- Configuration works both locally and in Docker.
- Validation and error handling cover expected failure paths.
- `dotnet build` succeeds.
- `dotnet test` succeeds.
- The changed files and design can be explained clearly.
