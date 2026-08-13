using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Persistence.Repositories;

public class SubjectRepository : Repository<Subject>, ISubjectRepository
{
    public SubjectRepository(ApplicationDbContext context) : base(context) { }

    public async Task<Subject?> GetWithClassAsync(Guid id, CancellationToken ct = default)
        => await Set.Include(s => s.ClassCourse)
                    .FirstOrDefaultAsync(s => s.Id == id, ct);

    public async Task<IReadOnlyList<Subject>> ListAsync(Guid? classCourseId, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Include(s => s.ClassCourse).AsQueryable();
        if (classCourseId.HasValue)
            query = query.Where(s => s.ClassCourseId == classCourseId.Value);
        return await query.OrderBy(s => s.Name).ToListAsync(ct);
    }

    public async Task<bool> CodeExistsInClassAsync(string code, Guid classCourseId, Guid? excludeId = null, CancellationToken ct = default)
        => await Set.AnyAsync(s => s.ClassCourseId == classCourseId && s.Code == code
                                   && (excludeId == null || s.Id != excludeId), ct);
}
