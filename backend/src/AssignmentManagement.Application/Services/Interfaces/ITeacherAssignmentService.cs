using AssignmentManagement.Application.DTOs.TeacherAssignments;

namespace AssignmentManagement.Application.Services.Interfaces;

public interface ITeacherAssignmentService
{
    Task<IReadOnlyList<TeacherAssignmentDto>> ListAsync(Guid? teacherId, CancellationToken ct = default);
    Task<TeacherAssignmentDto> AssignAsync(AssignTeacherRequest request, CancellationToken ct = default);
    Task UnassignAsync(Guid id, CancellationToken ct = default);
}
