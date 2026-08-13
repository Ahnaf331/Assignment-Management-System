using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Persistence.Repositories;

public class TeacherAssignmentRepository : Repository<TeacherAssignment>, ITeacherAssignmentRepository
{
    public TeacherAssignmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExistsAsync(Guid teacherId, Guid subjectId, Guid classCourseId, CancellationToken ct = default)
        => await Set.AnyAsync(t => t.TeacherId == teacherId && t.SubjectId == subjectId && t.ClassCourseId == classCourseId, ct);

    public async Task<bool> TeacherTeachesSubjectInClassAsync(Guid teacherId, Guid subjectId, Guid classCourseId, CancellationToken ct = default)
        => await Set.AnyAsync(t => t.TeacherId == teacherId && t.SubjectId == subjectId && t.ClassCourseId == classCourseId, ct);

    public async Task<IReadOnlyList<TeacherAssignment>> ListDetailedAsync(Guid? teacherId, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking()
            .Include(t => t.Teacher)
            .Include(t => t.Subject)
            .Include(t => t.ClassCourse)
            .AsQueryable();

        if (teacherId.HasValue)
            query = query.Where(t => t.TeacherId == teacherId.Value);

        return await query
            .OrderBy(t => t.ClassCourse.Name)
            .ThenBy(t => t.Subject.Name)
            .ToListAsync(ct);
    }

    public async Task<TeacherAssignment?> GetDetailedAsync(Guid id, CancellationToken ct = default)
        => await Set.AsNoTracking()
            .Include(t => t.Teacher)
            .Include(t => t.Subject)
            .Include(t => t.ClassCourse)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
}
