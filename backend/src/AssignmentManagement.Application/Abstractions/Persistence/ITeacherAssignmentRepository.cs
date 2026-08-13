using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Application.Abstractions.Persistence;

public interface ITeacherAssignmentRepository : IRepository<TeacherAssignment>
{
    Task<bool> ExistsAsync(Guid teacherId, Guid subjectId, Guid classCourseId, CancellationToken ct = default);
    Task<bool> TeacherTeachesSubjectInClassAsync(Guid teacherId, Guid subjectId, Guid classCourseId, CancellationToken ct = default);
    Task<IReadOnlyList<TeacherAssignment>> ListDetailedAsync(Guid? teacherId, CancellationToken ct = default);
    Task<TeacherAssignment?> GetDetailedAsync(Guid id, CancellationToken ct = default);
}
