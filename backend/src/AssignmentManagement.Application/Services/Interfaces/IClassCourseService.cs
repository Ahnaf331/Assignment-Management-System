using AssignmentManagement.Application.DTOs.Classes;

namespace AssignmentManagement.Application.Services.Interfaces;

public interface IClassCourseService
{
    Task<IReadOnlyList<ClassCourseDto>> ListAsync(string? search, CancellationToken ct = default);
    Task<ClassCourseDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ClassCourseDto> CreateAsync(CreateClassCourseRequest request, CancellationToken ct = default);
    Task<ClassCourseDto> UpdateAsync(Guid id, UpdateClassCourseRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
