using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.DTOs.Subjects;
using AssignmentManagement.Application.Mapping;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Exceptions;

namespace AssignmentManagement.Application.Services;

public class SubjectService : ISubjectService
{
    private readonly IUnitOfWork _uow;

    public SubjectService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<SubjectDto>> ListAsync(Guid? classCourseId, CancellationToken ct = default)
    {
        var subjects = await _uow.Subjects.ListAsync(classCourseId, ct);
        return subjects.Select(s => s.ToDto()).ToList();
    }

    public async Task<SubjectDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _uow.Subjects.GetWithClassAsync(id, ct)
                      ?? throw new NotFoundException("Subject", id);
        return subject.ToDto();
    }

    public async Task<SubjectDto> CreateAsync(CreateSubjectRequest request, CancellationToken ct = default)
    {
        if (await _uow.Classes.GetByIdAsync(request.ClassCourseId, ct) is null)
            throw new NotFoundException("ClassCourse", request.ClassCourseId);

        var code = request.Code.Trim();
        if (await _uow.Subjects.CodeExistsInClassAsync(code, request.ClassCourseId, null, ct))
            throw new ConflictException($"Subject code '{code}' already exists in this class/course.");

        var subject = new Subject
        {
            Name = request.Name.Trim(),
            Code = code,
            ClassCourseId = request.ClassCourseId
        };

        await _uow.Subjects.AddAsync(subject, ct);
        await _uow.SaveChangesAsync(ct);
        return (await _uow.Subjects.GetWithClassAsync(subject.Id, ct))!.ToDto();
    }

    public async Task<SubjectDto> UpdateAsync(Guid id, UpdateSubjectRequest request, CancellationToken ct = default)
    {
        var subject = await _uow.Subjects.GetWithClassAsync(id, ct)
                      ?? throw new NotFoundException("Subject", id);

        var code = request.Code.Trim();
        if (await _uow.Subjects.CodeExistsInClassAsync(code, subject.ClassCourseId, id, ct))
            throw new ConflictException($"Subject code '{code}' already exists in this class/course.");

        subject.Name = request.Name.Trim();
        subject.Code = code;
        subject.UpdatedAt = DateTime.UtcNow;

        _uow.Subjects.Update(subject);
        await _uow.SaveChangesAsync(ct);
        return subject.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var subject = await _uow.Subjects.GetByIdAsync(id, ct)
                      ?? throw new NotFoundException("Subject", id);
        _uow.Subjects.Remove(subject);
        await _uow.SaveChangesAsync(ct);
    }
}
