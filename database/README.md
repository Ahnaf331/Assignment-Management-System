# Database

PostgreSQL database for the Assignment & Submission Management System.

## Contents

| File         | Purpose                                                                    |
| ------------ | -------------------------------------------------------------------------- |
| `schema.sql` | Idempotent DDL generated from the EF Core migrations. Creates all tables, keys, indexes and the `__EFMigrationsHistory` table. |
| `seed.sql`   | Demo/sample data (users for every role, classes, subjects, teacher assignments, assignments and submissions). |

## Data model

```
classes (1) ──< subjects (1) ──< assignments (1) ──< submissions
   │                                   ▲                   │
   │                                   │                   │
   └──< users (students)               │            users (student)
        users (teachers) ──────────────┘
        teacher_assignments  (teacher × subject × class join)
```

- **users** — Admin, Teacher and Student accounts (single table, `Role` column). Students reference their `ClassCourseId`.
- **classes** — a class or course; owns subjects and enrolls students.
- **subjects** — a subject taught inside a class (`Code` unique per class).
- **teacher_assignments** — which teacher may create assignments for which subject/class.
- **assignments** — authored by a teacher for a class + subject; `Status` = Draft/Published.
- **submissions** — one per (assignment, student); holds content, status, marks and feedback.

Enum values are stored as integers:

| Enum              | Values                                          |
| ----------------- | ----------------------------------------------- |
| Role              | Admin=0, Teacher=1, Student=2                    |
| Assignment.Status | Draft=0, Published=1                             |
| Submission.Status | Submitted=0, Late=1, Graded=2, Returned=3        |

## Setup options

### Option A — automatic (recommended)

Just run the backend. On startup it applies EF Core migrations (creating the database
and tables) and seeds the demo data automatically. Nothing to do here.

```bash
cd backend/src/AssignmentManagement.API
dotnet run
```

### Option B — manual with psql

```bash
# 1. Create the database
psql -U postgres -c "CREATE DATABASE assignment_management;"

# 2. Create the schema
psql -U postgres -d assignment_management -f schema.sql

# 3. Load the demo data
psql -U postgres -d assignment_management -f seed.sql
```

### Option C — EF Core CLI

```bash
cd backend
dotnet ef database update \
  --project src/AssignmentManagement.Infrastructure \
  --startup-project src/AssignmentManagement.API
```

> Use **either** the automatic seeding (Option A) **or** `seed.sql` (Option B) — not both,
> to avoid duplicate teacher-assignment/assignment/submission rows.

## Demo credentials

| Role    | Email               | Password    |
| ------- | ------------------- | ----------- |
| Admin   | admin@school.edu    | Admin@123   |
| Teacher | teacher@school.edu  | Teacher@123 |
| Student | student@school.edu  | Student@123 |

Additional accounts: `teacher2@school.edu`, `student2@school.edu`, `student3@school.edu`
(same passwords by role).
