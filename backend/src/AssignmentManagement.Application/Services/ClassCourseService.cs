using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.DTOs.Classes;
using AssignmentManagement.Application.Mapping;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Exceptions;

namespace AssignmentManagement.Application.Services;

public class ClassCourseService : IClassCourseService
{
    private readonly IUnitOfWork _uow;

    public ClassCourseService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<ClassCourseDto>> ListAsync(string? search, CancellationToken ct = default)
    {
        var classes = await _uow.Classes.ListWithDetailsAsync(search, ct);
        return classes.Select(c => c.ToDto()).ToList();
    }

    public async Task<ClassCourseDetailDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.Classes.GetWithSubjectsAsync(id, ct)
                     ?? throw new NotFoundException("ClassCourse", id);
        return entity.ToDetailDto();
    }

    public async Task<ClassCourseDto> CreateAsync(CreateClassCourseRequest request, CancellationToken ct = default)
    {
        var code = request.Code.Trim();
        if (await _uow.Classes.CodeExistsAsync(code, null, ct))
            throw new ConflictException($"A class/course with code '{code}' already exists.");

        var entity = new ClassCourse
        {
            Name = request.Name.Trim(),
            Code = code,
            Description = request.Description?.Trim()
        };

        await _uow.Classes.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);
        return entity.ToDto(0, 0);
    }

    public async Task<ClassCourseDto> UpdateAsync(Guid id, UpdateClassCourseRequest request, CancellationToken ct = default)
    {
        var entity = await _uow.Classes.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("ClassCourse", id);

        var code = request.Code.Trim();
        if (await _uow.Classes.CodeExistsAsync(code, id, ct))
            throw new ConflictException($"A class/course with code '{code}' already exists.");

        entity.Name = request.Name.Trim();
        entity.Code = code;
        entity.Description = request.Description?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        _uow.Classes.Update(entity);
        await _uow.SaveChangesAsync(ct);

        var refreshed = await _uow.Classes.GetWithSubjectsAsync(id, ct);
        return refreshed!.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.Classes.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("ClassCourse", id);

        var studentCount = await _uow.Classes.CountStudentsAsync(id, ct);
        if (studentCount > 0)
            throw new BusinessRuleException("Cannot delete a class/course that still has enrolled students.");

        _uow.Classes.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }
}
