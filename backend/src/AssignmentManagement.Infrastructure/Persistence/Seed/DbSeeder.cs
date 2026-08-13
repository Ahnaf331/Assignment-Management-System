using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AssignmentManagement.Infrastructure.Persistence.Seed;

/// <summary>
/// Idempotent seeding of demo data (users for every role, classes, subjects,
/// teacher assignments, published/draft assignments and sample submissions).
/// Runs only when the database has no users yet.
/// </summary>
public static class DbSeeder
{
    // Deterministic IDs so the seed data matches the SQL script in /database.
    private static readonly Guid AdminId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid Teacher1Id = Guid.Parse("22222222-2222-2222-2222-222222222221");
    private static readonly Guid Teacher2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static readonly Guid Student1Id = Guid.Parse("33333333-3333-3333-3333-333333333331");
    private static readonly Guid Student2Id = Guid.Parse("33333333-3333-3333-3333-333333333332");
    private static readonly Guid Student3Id = Guid.Parse("33333333-3333-3333-3333-333333333333");

    private static readonly Guid Class1Id = Guid.Parse("44444444-4444-4444-4444-444444444441");
    private static readonly Guid Class2Id = Guid.Parse("44444444-4444-4444-4444-444444444442");

    private static readonly Guid MathId = Guid.Parse("55555555-5555-5555-5555-555555555551");
    private static readonly Guid PhysicsId = Guid.Parse("55555555-5555-5555-5555-555555555552");
    private static readonly Guid CsId = Guid.Parse("55555555-5555-5555-5555-555555555553");

    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher hasher, ILogger logger)
    {
        if (await context.Users.AnyAsync())
        {
            logger.LogInformation("Database already seeded; skipping.");
            return;
        }

        logger.LogInformation("Seeding database with demo data...");

        // --- Classes ---
        var class1 = new ClassCourse { Id = Class1Id, Name = "Grade 10 - Section A", Code = "G10A", Description = "Grade 10 morning batch." };
        var class2 = new ClassCourse { Id = Class2Id, Name = "Computer Science 101", Code = "CS101", Description = "Intro to Computer Science." };
        context.Classes.AddRange(class1, class2);

        // --- Subjects ---
        var math = new Subject { Id = MathId, Name = "Mathematics", Code = "MATH", ClassCourseId = Class1Id };
        var physics = new Subject { Id = PhysicsId, Name = "Physics", Code = "PHY", ClassCourseId = Class1Id };
        var cs = new Subject { Id = CsId, Name = "Programming Fundamentals", Code = "PF", ClassCourseId = Class2Id };
        context.Subjects.AddRange(math, physics, cs);

        // --- Users ---
        var admin = new User { Id = AdminId, FullName = "System Administrator", Email = "admin@school.edu", PasswordHash = hasher.Hash("Admin@123"), Role = UserRole.Admin };
        var teacher1 = new User { Id = Teacher1Id, FullName = "Alice Teacher", Email = "teacher@school.edu", PasswordHash = hasher.Hash("Teacher@123"), Role = UserRole.Teacher };
        var teacher2 = new User { Id = Teacher2Id, FullName = "Bob Instructor", Email = "teacher2@school.edu", PasswordHash = hasher.Hash("Teacher@123"), Role = UserRole.Teacher };
        var student1 = new User { Id = Student1Id, FullName = "Charlie Student", Email = "student@school.edu", PasswordHash = hasher.Hash("Student@123"), Role = UserRole.Student, ClassCourseId = Class1Id };
        var student2 = new User { Id = Student2Id, FullName = "Dana Learner", Email = "student2@school.edu", PasswordHash = hasher.Hash("Student@123"), Role = UserRole.Student, ClassCourseId = Class1Id };
        var student3 = new User { Id = Student3Id, FullName = "Evan Pupil", Email = "student3@school.edu", PasswordHash = hasher.Hash("Student@123"), Role = UserRole.Student, ClassCourseId = Class2Id };
        context.Users.AddRange(admin, teacher1, teacher2, student1, student2, student3);

        // --- Teacher assignments (who teaches what, where) ---
        context.TeacherAssignments.AddRange(
            new TeacherAssignment { TeacherId = Teacher1Id, SubjectId = MathId, ClassCourseId = Class1Id },
            new TeacherAssignment { TeacherId = Teacher1Id, SubjectId = PhysicsId, ClassCourseId = Class1Id },
            new TeacherAssignment { TeacherId = Teacher2Id, SubjectId = CsId, ClassCourseId = Class2Id });

        // --- Assignments ---
        var publishedMath = new Assignment
        {
            Title = "Algebra Problem Set 1",
            Description = "Solve problems 1-20 from chapter 3. Show all working.",
            Deadline = DateTime.UtcNow.AddDays(7),
            MaxMarks = 100,
            Status = AssignmentStatus.Published,
            PublishedAt = DateTime.UtcNow,
            ClassCourseId = Class1Id,
            SubjectId = MathId,
            TeacherId = Teacher1Id,
            AllowResubmission = true,
            AllowLateSubmission = false
        };
        var draftPhysics = new Assignment
        {
            Title = "Newton's Laws Lab Report",
            Description = "Write up the results of the pendulum experiment.",
            Deadline = DateTime.UtcNow.AddDays(14),
            MaxMarks = 50,
            Status = AssignmentStatus.Draft,
            ClassCourseId = Class1Id,
            SubjectId = PhysicsId,
            TeacherId = Teacher1Id,
            AllowResubmission = true,
            AllowLateSubmission = true
        };
        var publishedCs = new Assignment
        {
            Title = "Hello World in C#",
            Description = "Write a console program that prints your name and today's date.",
            Deadline = DateTime.UtcNow.AddDays(3),
            MaxMarks = 20,
            Status = AssignmentStatus.Published,
            PublishedAt = DateTime.UtcNow,
            ClassCourseId = Class2Id,
            SubjectId = CsId,
            TeacherId = Teacher2Id,
            AllowResubmission = true,
            AllowLateSubmission = false
        };
        context.Assignments.AddRange(publishedMath, draftPhysics, publishedCs);

        // --- Submissions ---
        context.Submissions.AddRange(
            new Submission
            {
                AssignmentId = publishedMath.Id,
                StudentId = Student1Id,
                Content = "My answers: 1) x=4, 2) x=-2 ... (full working attached).",
                Status = SubmissionStatus.Submitted,
                SubmittedAt = DateTime.UtcNow.AddHours(-5)
            },
            new Submission
            {
                AssignmentId = publishedMath.Id,
                StudentId = Student2Id,
                Content = "Completed all 20 problems.",
                Status = SubmissionStatus.Graded,
                SubmittedAt = DateTime.UtcNow.AddHours(-10),
                Marks = 92,
                Feedback = "Excellent work, minor error on Q17.",
                GradedAt = DateTime.UtcNow.AddHours(-2),
                GradedById = Teacher1Id
            });

        await context.SaveChangesAsync();
        logger.LogInformation("Database seeding complete.");
    }
}
