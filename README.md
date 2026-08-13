# Assignment & Submission Management System

A role-based school/college web application where **teachers** create assignments,
**students** submit their work, and teachers **review, mark and give feedback** — with an
**admin** managing users, classes, subjects and teacher allocations. The UI is designed to
feel like **Google Classroom**.

> Built for the *Assistant Software Engineer Recruitment Project*.

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
- [Business rules enforced](#business-rules-enforced)
- [Assumptions](#assumptions)
- [Known limitations](#known-limitations)

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

```bash
# 1. Backend (creates the DB, applies migrations and seeds demo data automatically)
cd "backend/src/AssignmentManagement.API"
dotnet run
#   API:     http://localhost:5080
#   Swagger: http://localhost:5080/swagger

# 2. Frontend (in a second terminal)
cd frontend
npm install          # first time only
npm run dev
#   App: http://localhost:3000
```

Open http://localhost:3000, then sign in with any demo account below.

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
workflow (see [Business rules enforced](#business-rules-enforced)).

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

## Business rules enforced

- Authentication uses BCrypt-hashed passwords; deactivated accounts cannot log in.
- A teacher can only create an assignment for a subject/class they are **assigned** to.
- Students only see and can only submit to **published** assignments for **their own class**.
- A student cannot submit after the deadline unless late submissions are allowed
  (then it is flagged **Late**); one submission per student per assignment.
- A submission can only be updated **before the deadline**, when the assignment allows it,
  and not after it has been graded.
- Marks must be between `0` and the assignment's maximum marks.
- A teacher can only view/grade submissions for their **own** assignments.
- An assignment with existing submissions cannot be reverted to draft.

These are covered by the unit tests in `testing/AssignmentManagement.Tests`.

---

## Assumptions

- **One class per student.** Each student belongs to a single class/course
  (`ClassCourseId`), which keeps enrolment and "assignments for my class" simple.
- **Subjects belong to a class.** A subject is scoped to one class; its code is unique within
  that class. Teacher allocation is per (teacher, subject, class).
- **Submissions are text/links.** The answer is stored as text (which may include a link to
  externally-hosted work) rather than binary file upload.
- **Deleting users is a soft-delete** (deactivate) to preserve historical assignments and
  submissions; deleting an assignment cascades to its submissions.
- **Auto-migrate & seed on startup** for a smooth local setup. The committed connection string
  and JWT secret are **local-development defaults** — override them via environment variables
  (see `.env.example`) for any real deployment.
- Times are handled in **UTC** on the backend.

---

## Known limitations

- No file/attachment upload (submissions are text; a link can be pasted instead).
- No refresh-token rotation — a single JWT access token (8h default) is issued.
- Pagination is available on the API shape (`PagedResult`) but list endpoints currently
  return full lists filtered by search; large datasets would benefit from server-side paging.
- No email notifications (e.g. new assignment / graded) — surfaced in-app only.
- The auto-seed and `seed.sql` are alternatives; running both may duplicate the
  assignment/submission rows that use non-fixed IDs in the auto-seeder.
