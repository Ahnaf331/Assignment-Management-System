using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Application.DTOs.Assignments;
using AssignmentManagement.Application.Mapping;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AssignmentManagement.Application.Services;

public class AssignmentService : IAssignmentService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AssignmentService> _logger;

    public AssignmentService(IUnitOfWork uow, ICurrentUser currentUser, ILogger<AssignmentService> logger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IReadOnlyList<AssignmentDto>> ListForTeacherAsync(
        Guid? classCourseId, Guid? subjectId, AssignmentStatus? status, string? search, CancellationToken ct = default)
    {
        var items = await _uow.Assignments.ListForTeacherAsync(_currentUser.Id, classCourseId, subjectId, status, search, ct);
        return items.Select(a => a.ToDto()).ToList();
    }

    public async Task<IReadOnlyList<StudentAssignmentDto>> ListForStudentAsync(
        Guid? subjectId, string? search, CancellationToken ct = default)
    {
        var student = await _uow.Users.GetByIdAsync(_currentUser.Id, ct)
                      ?? throw new NotFoundException("User", _currentUser.Id);
        if (student.ClassCourseId is null)
            throw new BusinessRuleException("You are not enrolled in any class/course.");

        var items = await _uow.Assignments.ListPublishedForClassAsync(student.ClassCourseId.Value, subjectId, search, ct);

        var result = new List<StudentAssignmentDto>(items.Count);
        foreach (var a in items)
        {
            var own = await _uow.Submissions.GetByAssignmentAndStudentAsync(a.Id, student.Id, ct);
            result.Add(a.ToStudentDto(own));
        }
        return result;
    }

    public async Task<IReadOnlyList<AssignmentDto>> ListAllAsync(
        Guid? classCourseId, Guid? subjectId, AssignmentStatus? status, string? search, CancellationToken ct = default)
    {
        var items = await _uow.Assignments.ListAllDetailedAsync(classCourseId, subjectId, status, search, ct);
        return items.Select(a => a.ToDto()).ToList();
    }

    public async Task<AssignmentDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetDetailedAsync(id, ct)
                         ?? throw new NotFoundException("Assignment", id);

        await EnsureCanViewAsync(assignment, ct);
        return assignment.ToDto();
    }

    public async Task<AssignmentDto> CreateAsync(CreateAssignmentRequest request, CancellationToken ct = default)
    {
        var subject = await _uow.Subjects.GetByIdAsync(request.SubjectId, ct)
                      ?? throw new NotFoundException("Subject", request.SubjectId);

        if (subject.ClassCourseId != request.ClassCourseId)
            throw new BusinessRuleException("The subject does not belong to the selected class/course.");

        // Business rule: a teacher may only author assignments for subject/class pairs they are assigned to.
        var teaches = await _uow.TeacherAssignments.TeacherTeachesSubjectInClassAsync(
            _currentUser.Id, request.SubjectId, request.ClassCourseId, ct);
        if (!teaches)
            throw new ForbiddenException("You are not assigned to teach this subject in this class/course.");

        if (request.Deadline <= DateTime.UtcNow)
            throw new BusinessRuleException("The deadline must be in the future.");

        if (request.MaxMarks <= 0)
            throw new BusinessRuleException("Maximum marks must be greater than zero.");

        var assignment = new Assignment
        {
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Deadline = DateTime.SpecifyKind(request.Deadline, DateTimeKind.Utc),
            MaxMarks = request.MaxMarks,
            ClassCourseId = request.ClassCourseId,
            SubjectId = request.SubjectId,
            TeacherId = _currentUser.Id,
            AllowResubmission = request.AllowResubmission,
            AllowLateSubmission = request.AllowLateSubmission,
            Status = request.PublishImmediately ? AssignmentStatus.Published : AssignmentStatus.Draft,
            PublishedAt = request.PublishImmediately ? DateTime.UtcNow : null
        };

        await _uow.Assignments.AddAsync(assignment, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Teacher {TeacherId} created assignment {AssignmentId}", _currentUser.Id, assignment.Id);

        return (await _uow.Assignments.GetDetailedAsync(assignment.Id, ct))!.ToDto();
    }

    public async Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest request, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetDetailedAsync(id, ct)
                         ?? throw new NotFoundException("Assignment", id);
        EnsureOwner(assignment);

        if (request.MaxMarks <= 0)
            throw new BusinessRuleException("Maximum marks must be greater than zero.");

        // Prevent lowering max marks below marks already awarded.
        var maxAwarded = assignment.Submissions
            .Where(s => s.Marks.HasValue)
            .Select(s => s.Marks!.Value)
            .DefaultIfEmpty(0)
            .Max();
        if (request.MaxMarks < maxAwarded)
            throw new BusinessRuleException($"Maximum marks cannot be less than the highest mark already awarded ({maxAwarded}).");

        assignment.Title = request.Title.Trim();
        assignment.Description = request.Description.Trim();
        assignment.Deadline = DateTime.SpecifyKind(request.Deadline, DateTimeKind.Utc);
        assignment.MaxMarks = request.MaxMarks;
        assignment.AllowResubmission = request.AllowResubmission;
        assignment.AllowLateSubmission = request.AllowLateSubmission;
        assignment.UpdatedAt = DateTime.UtcNow;

        _uow.Assignments.Update(assignment);
        await _uow.SaveChangesAsync(ct);
        return assignment.ToDto();
    }

    public async Task<AssignmentDto> PublishAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetDetailedAsync(id, ct)
                         ?? throw new NotFoundException("Assignment", id);
        EnsureOwner(assignment);

        if (assignment.Status == AssignmentStatus.Published)
            throw new BusinessRuleException("The assignment is already published.");

        assignment.Status = AssignmentStatus.Published;
        assignment.PublishedAt = DateTime.UtcNow;
        assignment.UpdatedAt = DateTime.UtcNow;

        _uow.Assignments.Update(assignment);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Assignment {AssignmentId} published", assignment.Id);
        return assignment.ToDto();
    }

    public async Task<AssignmentDto> UnpublishAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetDetailedAsync(id, ct)
                         ?? throw new NotFoundException("Assignment", id);
        EnsureOwner(assignment);

        if (assignment.Status == AssignmentStatus.Draft)
            throw new BusinessRuleException("The assignment is already a draft.");

        if (assignment.Submissions.Any())
            throw new BusinessRuleException("Cannot revert to draft: students have already submitted to this assignment.");

        assignment.Status = AssignmentStatus.Draft;
        assignment.PublishedAt = null;
        assignment.UpdatedAt = DateTime.UtcNow;

        _uow.Assignments.Update(assignment);
        await _uow.SaveChangesAsync(ct);
        return assignment.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetDetailedAsync(id, ct)
                         ?? throw new NotFoundException("Assignment", id);

        // Owner teacher or an Admin may delete.
        if (_currentUser.Role != UserRole.Admin && assignment.TeacherId != _currentUser.Id)
            throw new ForbiddenException("You can only delete your own assignments.");

        _uow.Assignments.Remove(assignment);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Assignment {AssignmentId} deleted by {UserId}", assignment.Id, _currentUser.Id);
    }

    private void EnsureOwner(Assignment assignment)
    {
        if (assignment.TeacherId != _currentUser.Id)
            throw new ForbiddenException("You can only manage your own assignments.");
    }

    private async Task EnsureCanViewAsync(Assignment assignment, CancellationToken ct)
    {
        switch (_currentUser.Role)
        {
            case UserRole.Admin:
                return;
            case UserRole.Teacher:
                if (assignment.TeacherId != _currentUser.Id)
                    throw new ForbiddenException("You can only view your own assignments.");
                return;
            case UserRole.Student:
                var student = await _uow.Users.GetByIdAsync(_currentUser.Id, ct)
                              ?? throw new NotFoundException("User", _currentUser.Id);
                if (!assignment.IsPublished || assignment.ClassCourseId != student.ClassCourseId)
                    throw new ForbiddenException("This assignment is not available to you.");
                return;
            default:
                throw new ForbiddenException("This assignment is not available to you.");
        }
    }
}
