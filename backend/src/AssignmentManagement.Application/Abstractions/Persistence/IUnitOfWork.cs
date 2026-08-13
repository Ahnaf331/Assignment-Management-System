namespace AssignmentManagement.Application.Abstractions.Persistence;

/// <summary>
/// Coordinates the repositories and commits all changes in a single transaction.
/// </summary>
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    IClassCourseRepository Classes { get; }
    ISubjectRepository Subjects { get; }
    ITeacherAssignmentRepository TeacherAssignments { get; }
    IAssignmentRepository Assignments { get; }
    ISubmissionRepository Submissions { get; }

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
