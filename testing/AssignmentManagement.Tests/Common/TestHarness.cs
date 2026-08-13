using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Infrastructure.Persistence;
using AssignmentManagement.Infrastructure.Persistence.Repositories;
using AssignmentManagement.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Tests.Common;

/// <summary>
/// Spins up the real repositories/UnitOfWork over an EF Core in-memory database so the
/// service business rules are exercised through the same code paths used in production.
/// </summary>
public sealed class TestHarness : IDisposable
{
    public ApplicationDbContext Db { get; }
    public IUnitOfWork Uow { get; }
    public IPasswordHasher Hasher { get; } = new BcryptPasswordHasher();
    public FakeCurrentUser CurrentUser { get; } = new();

    // Well-known seed IDs used across tests.
    public Guid ClassId { get; } = Guid.NewGuid();
    public Guid OtherClassId { get; } = Guid.NewGuid();
    public Guid MathSubjectId { get; } = Guid.NewGuid();
    public Guid CsSubjectId { get; } = Guid.NewGuid();
    public Guid TeacherId { get; } = Guid.NewGuid();
    public Guid OtherTeacherId { get; } = Guid.NewGuid();
    public Guid StudentId { get; } = Guid.NewGuid();
    public Guid OtherClassStudentId { get; } = Guid.NewGuid();

    public TestHarness()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"amt-tests-{Guid.NewGuid()}")
            .EnableSensitiveDataLogging()
            .Options;

        Db = new ApplicationDbContext(options);
        Uow = new UnitOfWork(Db);
        SeedBaseline();
    }

    private void SeedBaseline()
    {
        Db.Classes.AddRange(
            new ClassCourse { Id = ClassId, Name = "Grade 10", Code = "G10" },
            new ClassCourse { Id = OtherClassId, Name = "CS 101", Code = "CS101" });

        Db.Subjects.AddRange(
            new Subject { Id = MathSubjectId, Name = "Mathematics", Code = "MATH", ClassCourseId = ClassId },
            new Subject { Id = CsSubjectId, Name = "Programming", Code = "PF", ClassCourseId = OtherClassId });

        Db.Users.AddRange(
            new User { Id = TeacherId, FullName = "Teacher One", Email = "t1@test.com", PasswordHash = Hasher.Hash("Teacher@123"), Role = UserRole.Teacher },
            new User { Id = OtherTeacherId, FullName = "Teacher Two", Email = "t2@test.com", PasswordHash = Hasher.Hash("Teacher@123"), Role = UserRole.Teacher },
            new User { Id = StudentId, FullName = "Student One", Email = "s1@test.com", PasswordHash = Hasher.Hash("Student@123"), Role = UserRole.Student, ClassCourseId = ClassId },
            new User { Id = OtherClassStudentId, FullName = "Student Two", Email = "s2@test.com", PasswordHash = Hasher.Hash("Student@123"), Role = UserRole.Student, ClassCourseId = OtherClassId });

        // Teacher One teaches Mathematics in Grade 10 only.
        Db.TeacherAssignments.Add(new TeacherAssignment { TeacherId = TeacherId, SubjectId = MathSubjectId, ClassCourseId = ClassId });

        Db.SaveChanges();
    }

    /// <summary>Adds an assignment authored by Teacher One for Math/Grade 10.</summary>
    public Assignment AddAssignment(AssignmentStatus status, DateTime? deadline = null,
        bool allowResubmission = true, bool allowLateSubmission = false, int maxMarks = 100)
    {
        var assignment = new Assignment
        {
            Title = "Test Assignment",
            Description = "Do the work.",
            Deadline = deadline ?? DateTime.UtcNow.AddDays(3),
            MaxMarks = maxMarks,
            Status = status,
            PublishedAt = status == AssignmentStatus.Published ? DateTime.UtcNow : null,
            ClassCourseId = ClassId,
            SubjectId = MathSubjectId,
            TeacherId = TeacherId,
            AllowResubmission = allowResubmission,
            AllowLateSubmission = allowLateSubmission
        };
        Db.Assignments.Add(assignment);
        Db.SaveChanges();
        Db.ChangeTracker.Clear();
        return assignment;
    }

    public void AsTeacher() => CurrentUser.SetUser(TeacherId, UserRole.Teacher);
    public void AsOtherTeacher() => CurrentUser.SetUser(OtherTeacherId, UserRole.Teacher);
    public void AsStudent() => CurrentUser.SetUser(StudentId, UserRole.Student);
    public void AsOtherClassStudent() => CurrentUser.SetUser(OtherClassStudentId, UserRole.Student);

    public void Dispose() => Db.Dispose();
}
