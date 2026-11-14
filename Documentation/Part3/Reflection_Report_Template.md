# Part 3 Technical Reflection

**Student:** [Your name]  
**Student number:** ST10543971  
**Module:** [Module name/code]  
**Repository:** https://github.com/ST10543971/ST10543971_EAPD7111-POE_PART_2  
**Submission filename:** `ST10543971_Part3_Reflection.pdf`  
**Referencing style:** Harvard  
**Word count:** [Insert final word count, excluding references]

> **Important:** This is a planning template, not a finished submission. Replace every bracketed instruction with your own observations and wording. Verify every source, URL, publication detail, and access date before exporting to PDF. Do not claim that a Docker runtime test or screenshot exists until you have performed and captured it.

## 1. Introduction

[Write 2–3 sentences in your own words. Explain what the TechMove logistics management system does and what changed in Part 3. Mention the separation into the `TechMove.Api` backend, the `TechMove` MVC frontend, automated tests in `TechMove.Tests`, and Docker Compose deployment. Cite external definitions or factual claims about the technologies, but identify your own implementation observations as project evidence.]

**Project evidence to insert:**

- [Screenshot or repository reference showing the solution projects.]
- [Briefly explain the most important difference from Part 2.]

## 2. Automated testing in a CI/CD pipeline

### 2.1 CI/CD concepts

[Define continuous integration in one sentence and continuous delivery/deployment in one sentence. Use an authoritative source such as Fowler or Humble and Farley, with Harvard in-text citations. Use your own wording rather than copying a definition.]

### 2.2 Why automated tests matter

[Explain why manual-only testing becomes slow, inconsistent, and difficult to repeat as an application grows. Link the explanation to this project: API endpoints, authentication, contract rules, file uploads, and currency conversion can regress independently. Cite external claims and distinguish them from your own observations.]

### 2.3 How the pipeline gates changes

[Describe the pipeline implemented in `.github/workflows/ci.yml`:

`push/pull request → checkout → .NET 10 setup → restore → Release build → tests → test-result artifact → Docker Compose image build`.

Explain that the Docker build job depends on the test job, so a failed test prevents the image-build job from running. Do not state that deployment occurs unless you add and verify a deployment job.]

**Evidence:**

- Workflow file: `.github/workflows/ci.yml`
- Screenshot: `[Insert GitHub Actions run showing the green test job and Docker build job]`
- Run/date/commit: `[Insert exact evidence]`

### 2.4 Tests that protect TechMove behaviour

Use concrete tests from the repository and explain the risk each one reduces:

- `TechMove.Tests/WorkflowTests.cs`: `ExpiredContractCannotCreateRequest` protects the rule that service requests cannot be raised against expired contracts.
- `TechMove.Tests/Integration/AgreementsServiceRatesTests.cs`: `CreateThenGet_ServiceRequest_ReturnsSameData` checks that data survives the create/read API path and that response mapping/serialization remains compatible.
- `TechMove.Tests/Integration/AuthAndContractsTests.cs`: `Login_WithWrongPassword_Returns401` checks an authentication failure path.
- `[Add the exact 403 test name after verifying it in Test Explorer or the test files.]`
- `[Add the exact test result: total, passed, failed, skipped, date, and commit.]`

[Explain one regression that could occur without these tests and how the pipeline would detect it. Do not claim a test caught a historical bug unless you have evidence; describe it as the risk the test is designed to prevent.]

### 2.5 Limits and improvements

[Discuss limits: tests prove only asserted behaviour; coverage may be incomplete; integration tests can be slower; flaky tests reduce trust; external APIs and production infrastructure need additional testing. Suggest one realistic improvement, such as coverage reporting, contract tests, a SQL Server integration environment, or a controlled live exchange-rate test. Cite external ideas where appropriate.]

## 3. Containerisation and “works on my machine”

### 3.1 Concepts

[In your own words, distinguish a Docker image from a running container. Explain what a Dockerfile captures and what `docker-compose.yml` captures. Cite Docker or Microsoft documentation for factual definitions.]

### 3.2 The consistency problem

[Explain how SDK/runtime versions, SQL Server installation state, file paths, ports, and environment settings can differ between machines. Link this to the original application and explain why a reproducible image helps. Cite a source for general containerisation claims.]

### 3.3 TechMove implementation

Describe only what you have verified:

- `TechMove.Api/Dockerfile` uses `mcr.microsoft.com/dotnet/sdk:10.0` to restore/publish and `mcr.microsoft.com/dotnet/aspnet:10.0` at runtime.
- The root `Dockerfile` builds `TechMove` separately and uses `TechMove.dll` as its entrypoint.
- Both Dockerfiles use multi-stage builds; the SDK stage is not shipped in the runtime image.
- The API creates `/app/Uploads`, assigns it to the non-root `app` user, and mounts `agreements-data` there.
- `docker-compose.yml` defines `sql-server-db`, `techmove-api`, and `techmove-web` on `techmove-network`.
- The frontend uses `http://techmove-api:8080`, demonstrating service-name DNS rather than `localhost` between containers.
- SQL startup is gated by a healthcheck; SQL data and agreement files use named volumes.
- Secrets and environment-specific values come from `.env`/Compose variables, while `.env` is ignored by Git.

**Evidence to insert:**

- Screenshot: `[Docker Desktop Containers tab]`
- Screenshot: `[Docker Desktop Images tab]`
- Screenshot: `[Docker Desktop Volumes tab]`
- Screenshot: `[Compose logs showing API startup/migrations]`
- Verification command/output: `[Insert actual command and result]`

### 3.4 Trade-offs

[Discuss image size and build time, persistent volume management, backup/recovery, secret handling, and the difference between Compose and production orchestration such as Kubernetes. Explain that secrets should not be baked into an image. Cite authoritative sources for externally sourced claims.]

## 4. How testing and containers work together

[Write 2–3 sentences in your own words. Explain how CI can build and test the solution before building images, and how the same image can then be promoted between environments with configuration supplied externally. Be precise about what this repository actually automates: the current workflow runs restore, build, tests, and `docker compose build`; it does not deploy to production unless you add that separately.]

## 5. Conclusion

[Write 2–3 sentences summarising what you learned, the strongest reliability improvement, and one limitation or next step. Keep this personal and specific to your implementation.]

## 6. Reference list

Use one consistent Harvard format. Verify each source exists and read it before citing it. Replace `[access date]` with the date you actually accessed the page.

Docker Docs (n.d.) *Multi-stage builds*. Available at: https://docs.docker.com/build/building/multi-stage/ (Accessed: [day month year]).

Docker Docs (n.d.) *Networking in Compose*. Available at: https://docs.docker.com/compose/how-tos/networking/ (Accessed: [day month year]).

Docker Docs (n.d.) *Compose file reference*. Available at: https://docs.docker.com/reference/compose-file/ (Accessed: [day month year]).

Microsoft (n.d.) *Integration tests in ASP.NET Core*. Microsoft Learn. Available at: [Verify and insert the exact Microsoft Learn URL] (Accessed: [day month year]).

Microsoft (n.d.) *Containerize a .NET app*. Microsoft Learn. Available at: [Verify and insert the exact Microsoft Learn URL] (Accessed: [day month year]).

Microsoft (n.d.) *.NET container images*. Microsoft Learn. Available at: [Verify and insert the exact Microsoft Learn URL] (Accessed: [day month year]).

Fowler, M. (2006) *Continuous Integration*. Available at: https://martinfowler.com/articles/continuousIntegration.html (Accessed: [day month year]).

Humble, J. and Farley, D. (2010) *Continuous Delivery*. Addison-Wesley. [Verify edition and publisher details against the copy/source you used.]

Merkel, D. (2014) ‘Docker: lightweight Linux containers for consistent development and deployment’, *Linux Journal*. [Verify issue, URL, and access date before citing.]

## Final author and submission checks

- [ ] All bracketed prompts and placeholders have been replaced.
- [ ] The wording reflects my own understanding and experience.
- [ ] Every external factual claim has an in-text Harvard citation.
- [ ] Every in-text citation appears in the reference list, and every listed source is cited.
- [ ] URLs, titles, authors, publication details, and access dates have been verified.
- [ ] Direct quotations are minimal, marked with quotation marks, and referenced.
- [ ] Actual project names and test names are used.
- [ ] Docker claims are supported by screenshots or command output captured by me.
- [ ] The report is approximately 600–900 words, excluding references, unless the module guide says otherwise.
- [ ] Exported PDF filename is `ST10543971_Part3_Reflection.pdf`.
- [ ] GitHub link is on the cover page.
