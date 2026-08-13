using AssignmentManagement.Application.Abstractions.Persistence;

namespace AssignmentManagement.Infrastructure.Persistence.Repositories;

/// <summary>
/// Wraps the DbContext and exposes the aggregate repositories. A single SaveChanges
/// commits all pending work as one transaction.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
        Users = new UserRepository(context);
        Classes = new ClassCourseRepository(context);
        Subjects = new SubjectRepository(context);
        TeacherAssignments = new TeacherAssignmentRepository(context);
        Assignments = new AssignmentRepository(context);
        Submissions = new SubmissionRepository(context);
    }

    public IUserRepository Users { get; }
    public IClassCourseRepository Classes { get; }
    public ISubjectRepository Subjects { get; }
    public ITeacherAssignmentRepository TeacherAssignments { get; }
    public IAssignmentRepository Assignments { get; }
    public ISubmissionRepository Submissions { get; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default) => _context.SaveChangesAsync(ct);
}
