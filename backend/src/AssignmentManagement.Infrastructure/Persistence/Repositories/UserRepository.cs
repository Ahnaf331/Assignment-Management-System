using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AssignmentManagement.Infrastructure.Persistence.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        => await Set.Include(u => u.ClassCourse)
                    .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<User?> GetByIdWithClassAsync(Guid id, CancellationToken ct = default)
        => await Set.Include(u => u.ClassCourse)
                    .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default)
        => await Set.AnyAsync(u => u.Email == email && (excludeId == null || u.Id != excludeId), ct);

    public async Task<IReadOnlyList<User>> ListAsync(UserRole? role, string? search, CancellationToken ct = default)
    {
        var query = Set.AsNoTracking().Include(u => u.ClassCourse).AsQueryable();

        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }

        return await query.OrderBy(u => u.FullName).ToListAsync(ct);
    }
}
