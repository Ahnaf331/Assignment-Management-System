using AssignmentManagement.Application.DTOs.Assignments;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Services.Interfaces;

public interface IAssignmentService
{
    // Teacher
    Task<IReadOnlyList<AssignmentDto>> ListForTeacherAsync(
        Guid? classCourseId, Guid? subjectId, AssignmentStatus? status, string? search, CancellationToken ct = default);

    // Student
    Task<IReadOnlyList<StudentAssignmentDto>> ListForStudentAsync(
        Guid? subjectId, string? search, CancellationToken ct = default);

    // Admin
    Task<IReadOnlyList<AssignmentDto>> ListAllAsync(
        Guid? classCourseId, Guid? subjectId, AssignmentStatus? status, string? search, CancellationToken ct = default);

    Task<AssignmentDto> GetByIdAsync(Guid id, CancellationToken ct = default);

    Task<AssignmentDto> CreateAsync(CreateAssignmentRequest request, CancellationToken ct = default);
    Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest request, CancellationToken ct = default);
    Task<AssignmentDto> PublishAsync(Guid id, CancellationToken ct = default);
    Task<AssignmentDto> UnpublishAsync(Guid id, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
