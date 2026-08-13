# Assignment & Submission Management System

A role-based school/college web application where **teachers** create assignments,
**students** submit their work, and teachers **review, mark and give feedback** — with an
**admin** managing users, classes, subjects and teacher allocations. 


---

## Table of contents

- [Main features](#main-features)
- [Technology stack](#technology-stack)
- [Project structure](#project-structure)
- [Architecture & design decisions](#architecture--design-decisions)
- [Prerequisites](#prerequisites)
- [Quick start](#quick-start)
- [Database setup](#database-setup)
- [Running the backend](#running-the-backend)
- [Running the frontend](#running-the-frontend)
- [Running with Visual Studio](#running-with-visual-studio)
- [Running the tests](#running-the-tests)
- [Demo credentials](#demo-credentials)
- [API overview](#api-overview)

---

## Main features

**Admin**
- Manage users (create / edit / deactivate) for all roles.
- Manage classes/courses and subjects.
- Assign teachers to subjects within classes.
- View all assignments and submissions.

**Teacher**
- Create, update and delete assignments for the subjects/classes they are assigned to.
- Set title, description, deadline and maximum marks.
- Publish an assignment or keep it as a **draft**.
- View student submissions, assign marks and give feedback.
- Change a submission's status (e.g. return for revision).

**Student**
- View published assignments for their class only.
- Submit an answer and update it before the deadline (when allowed).
- Track submission status, marks and teacher feedback.

**Cross-cutting**
- JWT authentication with role-based authorization enforced by the backend.
- Validation, consistent error responses, structured logging and Swagger/OpenAPI docs.
- Responsive Google Classroom-style UI.
- Unit tests covering business rules, authorization and the submission workflow.

---

## Technology stack

| Layer      | Technology                                                                 |
| ---------- | -------------------------------------------------------------------------- |
| Frontend   | Next.js 14 (App Router), React 18, TypeScript, Tailwind CSS, Axios         |
| Backend    | ASP.NET Core Web API (.NET 8), C#, Clean Architecture, Repository + Unit of Work |
| Database   | PostgreSQL, Entity Framework Core 8 (code-first migrations)                |
| Auth       | JWT bearer tokens, role-based authorization, BCrypt password hashing       |
| Validation | FluentValidation                                                           |
| Logging    | Serilog (console + rolling file)                                           |
| API docs   | Swagger / OpenAPI (Swashbuckle)                                            |
| Testing    | xUnit, FluentAssertions, Moq, EF Core InMemory                             |

---

## Project structure

```
Assignment Management System/
├── backend/                        # ASP.NET Core solution
│   ├── AssignmentManagement.sln
│   └── src/
│       ├── AssignmentManagement.Domain/          # Entities, enums, domain exceptions
│       ├── AssignmentManagement.Application/      # DTOs, service interfaces + implementations,
│       │                                          # repository abstractions, validators (SOLID)
│       ├── AssignmentManagement.Infrastructure/   # EF Core DbContext, repositories, UoW,
│       │                                          # JWT + BCrypt, migrations, DB seeder
│       └── AssignmentManagement.API/              # Controllers, middleware, Program.cs, Swagger
│
├── frontend/                       # Next.js + TypeScript app (Google Classroom style)
│   ├── app/                        # App Router pages (login, dashboard, assignments, admin…)
│   ├── components/                 # Shared UI (AppShell, cards, badges, modal…)
│   └── lib/                        # API client, auth context, types
│
├── database/                       # schema.sql, seed.sql, database README
│
├── testing/
│   └── AssignmentManagement.Tests/ # xUnit unit tests
│
├── .env.example                    # Combined env var reference
└── README.md
```

The test project is included in `backend/AssignmentManagement.sln`, so opening the solution in
Visual Studio gives you the API, all class libraries **and** the tests in one place.

---

## Architecture & design decisions

- **Clean Architecture** with four projects and a strict dependency direction:
  `API → Application → Domain` and `Infrastructure → Application/Domain`. The Domain has no
  dependencies; the Application defines interfaces that Infrastructure implements
  (Dependency Inversion).
- **Repository + Unit of Work.** Each aggregate has a focused repository interface
  (`IUserRepository`, `IAssignmentRepository`, …) in the Application layer; EF Core
  implementations live in Infrastructure. `IUnitOfWork` groups them and commits atomically.
  This keeps the Application layer free of any ORM dependency.
- **SOLID.**
  - *SRP* — services own one area of business logic; mapping, validation, security are separate.
  - *OCP/DIP* — services depend on abstractions (`IUnitOfWork`, `IPasswordHasher`,
    `IJwtTokenGenerator`, `ICurrentUser`), so implementations can be swapped (and mocked in tests).
  - *ISP* — small, role-specific repository/service interfaces rather than one fat interface.
- **Single users table** with a `Role` discriminator — simplest model that satisfies the
  three roles; students carry a `ClassCourseId`.
- **Domain exceptions** (`NotFoundException`, `ForbiddenException`, `BusinessRuleException`,
  `ConflictException`) are translated to correct HTTP status codes by a global middleware,
  giving a consistent JSON error contract.
- **Authorization is enforced on the backend** at two levels: `[Authorize(Roles=…)]` on
  controllers/actions, plus ownership/enrolment checks inside the services (e.g. a teacher can
  only grade submissions for assignments they authored).

---

## Prerequisites

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- [PostgreSQL 14+](https://www.postgresql.org/download/) running on `localhost:5432`

### Configure your database password (one-time)

No real credentials are committed to this repository, so set your local PostgreSQL
password once before the first run. Either:

**Option 1 — local dev settings file (recommended):**

```bash
cd backend/src/AssignmentManagement.API
cp appsettings.Development.json.example appsettings.Development.json
# then edit appsettings.Development.json and replace YOUR_POSTGRES_PASSWORD
```

`appsettings.Development.json` is gitignored, so your password stays off GitHub.

**Option 2 — environment variable:**

```bash
# PowerShell
$env:ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=assignment_management;Username=postgres;Password=YOUR_PASSWORD"
```

The connection defaults to:

```
Host=localhost  Port=5432  Database=assignment_management  Username=postgres  Password=<yours>
```

> The `Jwt:Secret` in `appsettings.json` is a randomly generated **development** key.
> Generate your own for any real deployment (see [`.env.example`](.env.example)).

---

## Quick start

> **Step 1 is required.** No real credentials are committed to this repo, so you must
> supply your own PostgreSQL password once. Everything else is automatic — the backend
> creates the database, applies migrations and seeds demo data on first run.

```bash
# 1. Set your PostgreSQL password (ONE TIME - required)
cd backend/src/AssignmentManagement.API
copy appsettings.Development.json.example appsettings.Development.json   # Windows
# cp appsettings.Development.json.example appsettings.Development.json   # macOS/Linux
#   -> open appsettings.Development.json and replace YOUR_POSTGRES_PASSWORD
#      with the password for your local 'postgres' user.
#      This file is gitignored, so your password is never committed.

# 2. Backend (creates the DB, applies migrations and seeds demo data automatically)
dotnet run
#   API:     http://localhost:5080
#   Swagger: http://localhost:5080/swagger

# 3. Frontend (in a second terminal)
cd frontend
npm install          # first time only
npm run dev
#   App: http://localhost:3000
```

Open http://localhost:3000, then sign in with any demo account below.

If you skip step 1, the API stops on startup and prints exactly which file to edit —
so it fails loudly with instructions rather than silently.

---

## Database setup

You do **not** need to create tables manually. Choose one:

- **Automatic (default):** running the backend applies EF Core migrations (creating the
  `assignment_management` database and all tables) and seeds demo data on first run.
- **Manual SQL:** run [`database/schema.sql`](database/schema.sql) then
  [`database/seed.sql`](database/seed.sql) — see [`database/README.md`](database/README.md).
- **EF CLI:** `dotnet ef database update --project src/AssignmentManagement.Infrastructure --startup-project src/AssignmentManagement.API` (from `backend/`).

To point at a different database/credentials, override
`ConnectionStrings__DefaultConnection` (see `.env.example`) or edit
`backend/src/AssignmentManagement.API/appsettings.json`.

---

## Running the backend

```bash
cd backend
dotnet restore
dotnet build
cd src/AssignmentManagement.API
dotnet run
```

- REST API base URL: `http://localhost:5080/api`
- Interactive API docs (Swagger UI): `http://localhost:5080/swagger`
- Logs are written to the console and to `backend/src/AssignmentManagement.API/logs/`.

---

## Running the frontend

```bash
cd frontend
npm install
cp .env.example .env.local     # already provided; points at http://localhost:5080/api
npm run dev                    # development (hot reload) on http://localhost:3000
# or
npm run build && npm start     # production build
```

---

## Running with Visual Studio

1. Open **`backend/AssignmentManagement.sln`** in Visual Studio 2022 (17.8+).
2. Ensure PostgreSQL is running and the connection string in
   `AssignmentManagement.API/appsettings.json` is correct.
3. Set **`AssignmentManagement.API`** as the startup project and press **F5**
   (the `http` profile launches on `http://localhost:5080` and opens Swagger).
4. Run the tests from **Test Explorer** (the `AssignmentManagement.Tests` project is in the solution).
5. Start the frontend separately with `npm run dev` in the `frontend/` folder.

---

## Running the tests

```bash
cd backend
dotnet test
# or target the test project directly:
dotnet test ../testing/AssignmentManagement.Tests/AssignmentManagement.Tests.csproj
```

29 unit tests cover authentication, assignment authorization and the full submission
workflow — including deadline rules, late/resubmission handling, mark validation and
role-based access checks.

---

## Demo credentials

| Role    | Email               | Password    |
| ------- | ------------------- | ----------- |
| Admin   | admin@school.edu    | Admin@123   |
| Teacher | teacher@school.edu  | Teacher@123 |
| Student | student@school.edu  | Student@123 |

Extra accounts for richer demos (same password per role):
`teacher2@school.edu`, `student2@school.edu`, `student3@school.edu`.

On the login screen you can click a demo card to auto-fill its credentials.

---

## API overview

All endpoints are under `/api`. Full, interactive documentation is at `/swagger`.

| Area              | Endpoint (method)                                    | Roles            |
| ----------------- | ---------------------------------------------------- | ---------------- |
| Auth              | `POST /auth/login`, `GET /auth/me`, `POST /auth/change-password` | Public / Any |
| Users             | `GET/POST /users`, `GET/PUT/DELETE /users/{id}`      | Admin            |
| Classes           | `GET /classes`, `GET /classes/{id}`                  | Any (read)       |
|                   | `POST/PUT/DELETE /classes/{id}`                      | Admin            |
| Subjects          | `GET /subjects`, `GET /subjects/{id}`                | Any (read)       |
|                   | `POST/PUT/DELETE /subjects/{id}`                     | Admin            |
| Teacher allocation| `GET/POST /teacher-assignments`, `DELETE /{id}`      | Admin            |
|                   | `GET /teacher-assignments/mine`                      | Teacher          |
| Assignments       | `GET /assignments` (role-aware), `GET /assignments/{id}` | Any          |
|                   | `POST/PUT /assignments`, `.../publish`, `.../unpublish` | Teacher       |
|                   | `DELETE /assignments/{id}`                           | Teacher / Admin  |
| Submissions       | `POST/PUT /assignments/{id}/submissions`             | Student          |
|                   | `GET /assignments/{id}/submissions/mine`             | Student          |
|                   | `GET /submissions/mine`                              | Student          |
|                   | `GET /assignments/{id}/submissions`                  | Teacher / Admin  |
|                   | `POST /submissions/{id}/grade`, `PUT /submissions/{id}/status` | Teacher / Admin |

The `GET /assignments` list is role-aware: Admin sees everything, a Teacher sees the
assignments they authored, and a Student sees only **published** assignments for **their** class.

---
