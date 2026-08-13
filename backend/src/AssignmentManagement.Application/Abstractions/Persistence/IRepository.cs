using AssignmentManagement.Domain.Common;

namespace AssignmentManagement.Application.Abstractions.Persistence;

/// <summary>
/// Generic repository for basic write/read-by-key operations, shared by all aggregates.
/// Complex, entity-specific queries live on the derived repository interfaces so the
/// Application layer stays free of any ORM/query dependency (DIP + ISP).
/// </summary>
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> ListAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}
