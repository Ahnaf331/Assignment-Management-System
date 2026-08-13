using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Abstractions.Persistence;

public interface IAssignmentRepository : IRepository<Assignment>
{
    /// <summary>Loads an assignment with its class, subject, teacher and submissions.</summary>
    Task<Assignment?> GetDetailedAsync(Guid id, CancellationToken ct = default);

    /// <summary>Assignments authored by a teacher, optionally filtered.</summary>
    Task<IReadOnlyList<Assignment>> ListForTeacherAsync(
        Guid teacherId, Guid? classCourseId, Guid? subjectId, AssignmentStatus? status,
        string? search, CancellationToken ct = default);

    /// <summary>Published assignments for a student's class.</summary>
    Task<IReadOnlyList<Assignment>> ListPublishedForClassAsync(
        Guid classCourseId, Guid? subjectId, string? search, CancellationToken ct = default);

    /// <summary>Every assignment in the system (Admin view).</summary>
    Task<IReadOnlyList<Assignment>> ListAllDetailedAsync(
        Guid? classCourseId, Guid? subjectId, AssignmentStatus? status, string? search, CancellationToken ct = default);
}
