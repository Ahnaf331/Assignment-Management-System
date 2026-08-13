using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Persistence.Repositories;

public class ClassCourseRepository : Repository<ClassCourse>, IClassCourseRepository
{
    public ClassCourseRepository(ApplicationDbContext context) : base(context) { }

    public async Task<ClassCourse?> GetWithSubjectsAsync(Guid id, CancellationToken ct = default)
        => await Set.Include(c => c.Subjects)
                    .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IReadOnlyList<ClassCourse>> ListWithDetailsAsync(string? search, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking()
            .Include(c => c.Subjects)
            .Include(c => c.Students)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(term) || c.Code.ToLower().Contains(term));
        }

        return await query.OrderBy(c => c.Name).ToListAsync(ct);
    }

    public async Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => await Set.AnyAsync(c => c.Code == code && (excludeId == null || c.Id != excludeId), ct);

    public async Task<int> CountStudentsAsync(Guid classId, CancellationToken ct = default)
        => await Context.Users.CountAsync(u => u.ClassCourseId == classId, ct);
}
