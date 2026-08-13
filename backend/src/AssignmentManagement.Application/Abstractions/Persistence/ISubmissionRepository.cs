using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Application.Abstractions.Persistence;

public interface ISubmissionRepository : IRepository<Submission>
{
    Task<Submission?> GetDetailedAsync(Guid id, CancellationToken ct = default);
    Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId, CancellationToken ct = default);
    Task<IReadOnlyList<Submission>> ListByAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
    Task<IReadOnlyList<Submission>> ListByStudentAsync(Guid studentId, CancellationToken ct = default);
}
