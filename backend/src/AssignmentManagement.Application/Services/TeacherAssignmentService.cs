using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.DTOs.TeacherAssignments;
using AssignmentManagement.Application.Mapping;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Domain.Exceptions;

namespace AssignmentManagement.Application.Services;

/// <summary>Admin assigns teachers to teach specific subjects within specific classes.</summary>
public class TeacherAssignmentService : ITeacherAssignmentService
{
    private readonly IUnitOfWork _uow;

    public TeacherAssignmentService(IUnitOfWork uow) => _uow = uow;

    public async Task<IReadOnlyList<TeacherAssignmentDto>> ListAsync(Guid? teacherId, CancellationToken ct = default)
    {
        var items = await _uow.TeacherAssignments.ListDetailedAsync(teacherId, ct);
        return items.Select(t => t.ToDto()).ToList();
    }

    public async Task<TeacherAssignmentDto> AssignAsync(AssignTeacherRequest request, CancellationToken ct = default)
    {
        var teacher = await _uow.Users.GetByIdAsync(request.TeacherId, ct)
                      ?? throw new NotFoundException("Teacher", request.TeacherId);
        if (teacher.Role != UserRole.Teacher)
            throw new BusinessRuleException("The selected user is not a teacher.");

        var subject = await _uow.Subjects.GetByIdAsync(request.SubjectId, ct)
                      ?? throw new NotFoundException("Subject", request.SubjectId);

        if (subject.ClassCourseId != request.ClassCourseId)
            throw new BusinessRuleException("The selected subject does not belong to the selected class/course.");

        if (await _uow.TeacherAssignments.ExistsAsync(request.TeacherId, request.SubjectId, request.ClassCourseId, ct))
            throw new ConflictException("This teacher is already assigned to that subject and class.");

        var entity = new TeacherAssignment
        {
            TeacherId = request.TeacherId,
            SubjectId = request.SubjectId,
            ClassCourseId = request.ClassCourseId
        };

        await _uow.TeacherAssignments.AddAsync(entity, ct);
        await _uow.SaveChangesAsync(ct);

        return (await _uow.TeacherAssignments.GetDetailedAsync(entity.Id, ct))!.ToDto();
    }

    public async Task UnassignAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _uow.TeacherAssignments.GetByIdAsync(id, ct)
                     ?? throw new NotFoundException("TeacherAssignment", id);
        _uow.TeacherAssignments.Remove(entity);
        await _uow.SaveChangesAsync(ct);
    }
}
