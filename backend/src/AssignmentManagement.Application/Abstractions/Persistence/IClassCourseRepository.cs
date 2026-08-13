using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Application.Abstractions.Persistence;

public interface IClassCourseRepository : IRepository<ClassCourse>
{
    Task<ClassCourse?> GetWithSubjectsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ClassCourse>> ListWithDetailsAsync(string? search, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task<int> CountStudentsAsync(Guid classId, CancellationToken ct = default);
}
