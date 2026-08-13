using AssignmentManagement.Application.DTOs.Subjects;

namespace AssignmentManagement.Application.Services.Interfaces;

public interface ISubjectService
{
    Task<IReadOnlyList<SubjectDto>> ListAsync(Guid? classCourseId, CancellationToken ct = default);
    Task<SubjectDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default);
    Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
