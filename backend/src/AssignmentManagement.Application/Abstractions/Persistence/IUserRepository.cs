using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Abstractions.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByIdWithClassAsync(Guid id, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, Guid? excludeId = null, CancellationToken ct = default);
    Task<IReadOnlyList<User>> ListAsync(UserRole? role, string? search, CancellationToken ct = default);
}
