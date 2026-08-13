-- ============================================================================
-- Assignment & Submission Management System - Demo seed data
-- ============================================================================
-- Run this AFTER schema.sql against the `assignment_management` database.
--   psql -U postgres -d assignment_management -f seed.sql
--
-- NOTE: The backend also seeds this exact data automatically on first startup
--       (see DbSeeder). This script is provided so the database can be populated
--       without running the API.
--
-- Enum mappings (stored as integers):
--   Role:             Admin=0, Teacher=1, Student=2
--   Assignment.Status: Draft=0, Published=1
--   Submission.Status: Submitted=0, Late=1, Graded=2, Returned=3
--
-- Demo passwords (BCrypt hashed below):
--   Admin   -> admin@school.edu   / Admin@123
--   Teacher -> teacher@school.edu / Teacher@123
--   Student -> student@school.edu / Student@123
-- ============================================================================

-- ---------- Classes / Courses ----------
INSERT INTO classes ("Id", "Name", "Code", "Description", "CreatedAt") VALUES
  ('44444444-4444-4444-4444-444444444441', 'Grade 10 - Section A', 'G10A', 'Grade 10 morning batch.', now()),
  ('44444444-4444-4444-4444-444444444442', 'Computer Science 101', 'CS101', 'Intro to Computer Science.', now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------- Subjects ----------
INSERT INTO subjects ("Id", "Name", "Code", "ClassCourseId", "CreatedAt") VALUES
  ('55555555-5555-5555-5555-555555555551', 'Mathematics', 'MATH', '44444444-4444-4444-4444-444444444441', now()),
  ('55555555-5555-5555-5555-555555555552', 'Physics', 'PHY', '44444444-4444-4444-4444-444444444441', now()),
  ('55555555-5555-5555-5555-555555555553', 'Programming Fundamentals', 'PF', '44444444-4444-4444-4444-444444444442', now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------- Users ----------
-- Password hashes are BCrypt (work factor 11).
INSERT INTO users ("Id", "FullName", "Email", "PasswordHash", "Role", "IsActive", "ClassCourseId", "CreatedAt") VALUES
  ('11111111-1111-1111-1111-111111111111', 'System Administrator', 'admin@school.edu',
     '$2a$11$ksnKDpT0Bd99VtLV8Cv3sO20.E8.N0Cth2aRCjg9w5ApOKJ0KC59e', 0, TRUE, NULL, now()),
  ('22222222-2222-2222-2222-222222222221', 'Alice Teacher', 'teacher@school.edu',
     '$2a$11$4t1YHDXLQmWG8ZTmQ2QNR.avBdC6HfHyr10L1hujvrFHz4/ENStai', 1, TRUE, NULL, now()),
  ('22222222-2222-2222-2222-222222222222', 'Bob Instructor', 'teacher2@school.edu',
     '$2a$11$4t1YHDXLQmWG8ZTmQ2QNR.avBdC6HfHyr10L1hujvrFHz4/ENStai', 1, TRUE, NULL, now()),
  ('33333333-3333-3333-3333-333333333331', 'Charlie Student', 'student@school.edu',
     '$2a$11$IuVcK8u7lcHNXwlKdSXEIeE81u19C06qfXI/g82F858aZeejCTGIe', 2, TRUE, '44444444-4444-4444-4444-444444444441', now()),
  ('33333333-3333-3333-3333-333333333332', 'Dana Learner', 'student2@school.edu',
     '$2a$11$IuVcK8u7lcHNXwlKdSXEIeE81u19C06qfXI/g82F858aZeejCTGIe', 2, TRUE, '44444444-4444-4444-4444-444444444441', now()),
  ('33333333-3333-3333-3333-333333333333', 'Evan Pupil', 'student3@school.edu',
     '$2a$11$IuVcK8u7lcHNXwlKdSXEIeE81u19C06qfXI/g82F858aZeejCTGIe', 2, TRUE, '44444444-4444-4444-4444-444444444442', now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------- Teacher assignments (who teaches what, where) ----------
INSERT INTO teacher_assignments ("Id", "TeacherId", "SubjectId", "ClassCourseId", "CreatedAt") VALUES
  ('66666666-6666-6666-6666-666666666661', '22222222-2222-2222-2222-222222222221', '55555555-5555-5555-5555-555555555551', '44444444-4444-4444-4444-444444444441', now()),
  ('66666666-6666-6666-6666-666666666662', '22222222-2222-2222-2222-222222222221', '55555555-5555-5555-5555-555555555552', '44444444-4444-4444-4444-444444444441', now()),
  ('66666666-6666-6666-6666-666666666663', '22222222-2222-2222-2222-222222222222', '55555555-5555-5555-5555-555555555553', '44444444-4444-4444-4444-444444444442', now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------- Assignments ----------
INSERT INTO assignments ("Id", "Title", "Description", "Deadline", "MaxMarks", "Status",
                         "AllowResubmission", "AllowLateSubmission", "PublishedAt",
                         "ClassCourseId", "SubjectId", "TeacherId", "CreatedAt") VALUES
  ('77777777-7777-7777-7777-777777777771', 'Algebra Problem Set 1',
     'Solve problems 1-20 from chapter 3. Show all working.', now() + interval '7 days', 100, 1,
     TRUE, FALSE, now(),
     '44444444-4444-4444-4444-444444444441', '55555555-5555-5555-5555-555555555551', '22222222-2222-2222-2222-222222222221', now()),
  ('77777777-7777-7777-7777-777777777772', 'Newton''s Laws Lab Report',
     'Write up the results of the pendulum experiment.', now() + interval '14 days', 50, 0,
     TRUE, TRUE, NULL,
     '44444444-4444-4444-4444-444444444441', '55555555-5555-5555-5555-555555555552', '22222222-2222-2222-2222-222222222221', now()),
  ('77777777-7777-7777-7777-777777777773', 'Hello World in C#',
     'Write a console program that prints your name and today''s date.', now() + interval '3 days', 20, 1,
     TRUE, FALSE, now(),
     '44444444-4444-4444-4444-444444444442', '55555555-5555-5555-5555-555555555553', '22222222-2222-2222-2222-222222222222', now())
ON CONFLICT ("Id") DO NOTHING;

-- ---------- Submissions ----------
INSERT INTO submissions ("Id", "AssignmentId", "StudentId", "Content", "SubmittedAt", "Status",
                        "Marks", "Feedback", "GradedAt", "GradedById", "CreatedAt") VALUES
  ('88888888-8888-8888-8888-888888888881', '77777777-7777-7777-7777-777777777771', '33333333-3333-3333-3333-333333333331',
     'My answers: 1) x=4, 2) x=-2 ... (full working attached).', now() - interval '5 hours', 0,
     NULL, NULL, NULL, NULL, now()),
  ('88888888-8888-8888-8888-888888888882', '77777777-7777-7777-7777-777777777771', '33333333-3333-3333-3333-333333333332',
     'Completed all 20 problems.', now() - interval '10 hours', 2,
     92, 'Excellent work, minor error on Q17.', now() - interval '2 hours', '22222222-2222-2222-2222-222222222221', now())
ON CONFLICT ("Id") DO NOTHING;
