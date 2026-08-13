using AssignmentManagement.Application.DTOs.Submissions;

namespace AssignmentManagement.Application.Services.Interfaces;

public interface ISubmissionService
{
    // Student
    Task<SubmissionDto> SubmitAsync(Guid assignmentId, CreateSubmissionRequest request, CancellationToken ct = default);
    Task<SubmissionDto> UpdateOwnAsync(Guid assignmentId, UpdateSubmissionRequest request, CancellationToken ct = default);
    Task<SubmissionDto?> GetOwnAsync(Guid assignmentId, CancellationToken ct = default);
    Task<IReadOnlyList<SubmissionDto>> ListOwnAsync(CancellationToken ct = default);

    // Teacher
    Task<IReadOnlyList<SubmissionDto>> ListForAssignmentAsync(Guid assignmentId, CancellationToken ct = default);
    Task<SubmissionDto> GetByIdAsync(Guid submissionId, CancellationToken ct = default);
    Task<SubmissionDto> GradeAsync(Guid submissionId, GradeSubmissionRequest request, CancellationToken ct = default);
    Task<SubmissionDto> ChangeStatusAsync(Guid submissionId, UpdateSubmissionStatusRequest request, CancellationToken ct = default);
}
