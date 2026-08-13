using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Application.Abstractions.Persistence;

public interface ISubjectRepository : IRepository<Subject>
{
    Task<Subject?> GetWithClassAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Subject>> ListAsync(Guid? classCourseId, CancellationToken ct = default);
    Task<bool> CodeExistsInClassAsync(string code, Guid classCourseId, Guid? excludeId = null, CancellationToken ct = default);
}
