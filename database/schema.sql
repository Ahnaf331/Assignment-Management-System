CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;


DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE TABLE classes (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Code" character varying(30) NOT NULL,
        "Description" character varying(500),
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_classes" PRIMARY KEY ("Id")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE TABLE subjects (
        "Id" uuid NOT NULL,
        "Name" character varying(150) NOT NULL,
        "Code" character varying(30) NOT NULL,
        "ClassCourseId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_subjects" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_subjects_classes_ClassCourseId" FOREIGN KEY ("ClassCourseId") REFERENCES classes ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE TABLE users (
        "Id" uuid NOT NULL,
        "FullName" character varying(150) NOT NULL,
        "Email" character varying(256) NOT NULL,
        "PasswordHash" text NOT NULL,
        "Role" integer NOT NULL,
        "IsActive" boolean NOT NULL,
        "ClassCourseId" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_users" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_users_classes_ClassCourseId" FOREIGN KEY ("ClassCourseId") REFERENCES classes ("Id") ON DELETE SET NULL
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE TABLE assignments (
        "Id" uuid NOT NULL,
        "Title" character varying(200) NOT NULL,
        "Description" character varying(5000) NOT NULL,
        "Deadline" timestamp with time zone NOT NULL,
        "MaxMarks" integer NOT NULL,
        "Status" integer NOT NULL,
        "AllowResubmission" boolean NOT NULL,
        "AllowLateSubmission" boolean NOT NULL,
        "PublishedAt" timestamp with time zone,
        "ClassCourseId" uuid NOT NULL,
        "SubjectId" uuid NOT NULL,
        "TeacherId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_assignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_assignments_classes_ClassCourseId" FOREIGN KEY ("ClassCourseId") REFERENCES classes ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_assignments_subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES subjects ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_assignments_users_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE TABLE teacher_assignments (
        "Id" uuid NOT NULL,
        "TeacherId" uuid NOT NULL,
        "SubjectId" uuid NOT NULL,
        "ClassCourseId" uuid NOT NULL,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_teacher_assignments" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_teacher_assignments_classes_ClassCourseId" FOREIGN KEY ("ClassCourseId") REFERENCES classes ("Id") ON DELETE RESTRICT,
        CONSTRAINT "FK_teacher_assignments_subjects_SubjectId" FOREIGN KEY ("SubjectId") REFERENCES subjects ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_teacher_assignments_users_TeacherId" FOREIGN KEY ("TeacherId") REFERENCES users ("Id") ON DELETE CASCADE
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE TABLE submissions (
        "Id" uuid NOT NULL,
        "AssignmentId" uuid NOT NULL,
        "StudentId" uuid NOT NULL,
        "Content" character varying(20000) NOT NULL,
        "SubmittedAt" timestamp with time zone NOT NULL,
        "Status" integer NOT NULL,
        "Marks" integer,
        "Feedback" character varying(5000),
        "GradedAt" timestamp with time zone,
        "GradedById" uuid,
        "CreatedAt" timestamp with time zone NOT NULL,
        "UpdatedAt" timestamp with time zone,
        CONSTRAINT "PK_submissions" PRIMARY KEY ("Id"),
        CONSTRAINT "FK_submissions_assignments_AssignmentId" FOREIGN KEY ("AssignmentId") REFERENCES assignments ("Id") ON DELETE CASCADE,
        CONSTRAINT "FK_submissions_users_GradedById" FOREIGN KEY ("GradedById") REFERENCES users ("Id") ON DELETE SET NULL,
        CONSTRAINT "FK_submissions_users_StudentId" FOREIGN KEY ("StudentId") REFERENCES users ("Id") ON DELETE RESTRICT
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_assignments_ClassCourseId" ON assignments ("ClassCourseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_assignments_SubjectId" ON assignments ("SubjectId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_assignments_TeacherId" ON assignments ("TeacherId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_classes_Code" ON classes ("Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_subjects_ClassCourseId_Code" ON subjects ("ClassCourseId", "Code");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_submissions_AssignmentId_StudentId" ON submissions ("AssignmentId", "StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_submissions_GradedById" ON submissions ("GradedById");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_submissions_StudentId" ON submissions ("StudentId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_teacher_assignments_ClassCourseId" ON teacher_assignments ("ClassCourseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_teacher_assignments_SubjectId" ON teacher_assignments ("SubjectId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_teacher_assignments_TeacherId_SubjectId_ClassCourseId" ON teacher_assignments ("TeacherId", "SubjectId", "ClassCourseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE INDEX "IX_users_ClassCourseId" ON users ("ClassCourseId");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    CREATE UNIQUE INDEX "IX_users_Email" ON users ("Email");
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20260811110837_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20260811110837_InitialCreate', '8.0.8');
    END IF;
END $EF$;
COMMIT;

