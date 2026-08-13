using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Persistence.Repositories;

public class SubmissionRepository : Repository<Submission>, ISubmissionRepository
{
    public SubmissionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Submission?> GetDetailedAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .Include(s => s.GradedBy)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default)
        => await Set.FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentId == studentId, ct);

    public async Task<IReadOnlyList<Submission>> ListByAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .Include(s => s.GradedBy)
            .Where(s => s.AssignmentId == assignmentId)
            .OrderBy(s => s.Student.FullName)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Submission>> ListByStudentAsync(Guid studentId, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .Include(s => s.GradedBy)
            .Where(s => s.StudentId == studentId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(ct);
}
