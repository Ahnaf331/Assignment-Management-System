using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Persistence.Repositories;

public class AssignmentRepository : Repository<Assignment>, IAssignmentRepository
{
    public AssignmentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Assignment?> GetDetailedAsync(Guid id, CancellationToken ct = default)
        => await Set
            .Include(a => a.ClassCourse)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions)
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IReadOnlyList<Assignment>> ListForTeacherAsync(
        Guid teacherId, Guid? classCourseId, Guid? subjectId, AssignmentStatus? status,
        string? search, CancellationToken ct = default)
    {
        var query = BaseQuery().Where(a => a.TeacherId == teacherId);
        query = ApplyFilters(query, classCourseId, subjectId, status, search);
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Assignment>> ListPublishedForClassAsync(
        Guid classCourseId, Guid? subjectId, string? search, CancellationToken ct = default)
    {
        var query = BaseQuery()
            .Where(a => a.ClassCourseId == classCourseId && a.Status == AssignmentStatus.Published);
        query = ApplyFilters(query, null, subjectId, null, search);
        return await query.OrderBy(a => a.Deadline).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Assignment>> ListAllDetailedAsync(
        Guid? classCourseId, Guid? subjectId, AssignmentStatus? status, string? search, CancellationToken ct = default)
    {
        var query = ApplyFilters(BaseQuery(), classCourseId, subjectId, status, search);
        return await query.OrderByDescending(a => a.CreatedAt).ToListAsync(ct);
    }

    private IQueryable<Assignment> BaseQuery()
        => Set.AsNoTracking()
            .Include(a => a.ClassCourse)
            .Include(a => a.Subject)
            .Include(a => a.Teacher)
            .Include(a => a.Submissions);

    private static IQueryable<Assignment> ApplyFilters(
        IQueryable<Assignment> query, Guid? classCourseId, Guid? subjectId, AssignmentStatus? status, string? search)
    {
        if (classCourseId.HasValue)
            query = query.Where(a => a.ClassCourseId == classCourseId.Value);
        if (subjectId.HasValue)
            query = query.Where(a => a.SubjectId == subjectId.Value);
        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(a => a.Title.ToLower().Contains(term));
        }
        return query;
    }
}
