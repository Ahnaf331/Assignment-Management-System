using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Application.DTOs.Submissions;
using AssignmentManagement.Application.Mapping;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AssignmentManagement.Application.Services;

public class SubmissionService : ISubmissionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<SubmissionService> _logger;

    public SubmissionService(IUnitOfWork uow, ICurrentUser currentUser, ILogger<SubmissionService> logger)
    {
        _uow = uow;
        _currentUser = currentUser;
        _logger = logger;
    }

    // ---------------- Student ----------------

    public async Task<SubmissionDto> SubmitAsync(Guid assignmentId, CreateSubmissionRequest request, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetDetailedAsync(assignmentId, ct)
                         ?? throw new NotFoundException("Assignment", assignmentId);
        var student = await GetEnrolledStudentAsync(ct);

        EnsureStudentCanAccess(assignment, student);

        var existing = await _uow.Submissions.GetByAssignmentAndStudentAsync(assignmentId, student.Id, ct);
        if (existing is not null)
            throw new ConflictException("You have already submitted this assignment. Use update instead.");

        var isLate = DateTime.UtcNow > assignment.Deadline;
        if (isLate && !assignment.AllowLateSubmission)
            throw new BusinessRuleException("The deadline has passed and late submissions are not allowed.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BusinessRuleException("Submission content cannot be empty.");

        var submission = new Submission
        {
            AssignmentId = assignmentId,
            StudentId = student.Id,
            Content = request.Content.Trim(),
            SubmittedAt = DateTime.UtcNow,
            Status = isLate ? SubmissionStatus.Late : SubmissionStatus.Submitted
        };

        await _uow.Submissions.AddAsync(submission, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Student {StudentId} submitted assignment {AssignmentId} (late={Late})",
            student.Id, assignmentId, isLate);

        return (await _uow.Submissions.GetDetailedAsync(submission.Id, ct))!.ToDto();
    }

    public async Task<SubmissionDto> UpdateOwnAsync(Guid assignmentId, UpdateSubmissionRequest request, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetDetailedAsync(assignmentId, ct)
                         ?? throw new NotFoundException("Assignment", assignmentId);
        var student = await GetEnrolledStudentAsync(ct);
        EnsureStudentCanAccess(assignment, student);

        var submission = await _uow.Submissions.GetByAssignmentAndStudentAsync(assignmentId, student.Id, ct)
                         ?? throw new NotFoundException("You have not submitted this assignment yet.");

        if (!assignment.AllowResubmission)
            throw new BusinessRuleException("This assignment does not allow updating a submission.");

        // Business rule: a submission can only be updated before the deadline.
        if (DateTime.UtcNow > assignment.Deadline)
            throw new BusinessRuleException("The deadline has passed; the submission can no longer be updated.");

        if (submission.Status == SubmissionStatus.Graded)
            throw new BusinessRuleException("A graded submission can no longer be updated.");

        if (string.IsNullOrWhiteSpace(request.Content))
            throw new BusinessRuleException("Submission content cannot be empty.");

        submission.Content = request.Content.Trim();
        submission.SubmittedAt = DateTime.UtcNow;
        submission.UpdatedAt = DateTime.UtcNow;
        submission.Status = SubmissionStatus.Submitted; // fresh, on-time update

        _uow.Submissions.Update(submission);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Student {StudentId} updated submission {SubmissionId}", student.Id, submission.Id);

        return (await _uow.Submissions.GetDetailedAsync(submission.Id, ct))!.ToDto();
    }

    public async Task<SubmissionDto?> GetOwnAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var submission = await _uow.Submissions.GetByAssignmentAndStudentAsync(assignmentId, _currentUser.Id, ct);
        if (submission is null) return null;
        return (await _uow.Submissions.GetDetailedAsync(submission.Id, ct))!.ToDto();
    }

    public async Task<IReadOnlyList<SubmissionDto>> ListOwnAsync(CancellationToken ct = default)
    {
        var items = await _uow.Submissions.ListByStudentAsync(_currentUser.Id, ct);
        return items.Select(s => s.ToDto()).ToList();
    }

    // ---------------- Teacher ----------------

    public async Task<IReadOnlyList<SubmissionDto>> ListForAssignmentAsync(Guid assignmentId, CancellationToken ct = default)
    {
        var assignment = await _uow.Assignments.GetByIdAsync(assignmentId, ct)
                         ?? throw new NotFoundException("Assignment", assignmentId);
        EnsureTeacherOwnsAssignment(assignment);

        var items = await _uow.Submissions.ListByAssignmentAsync(assignmentId, ct);
        return items.Select(s => s.ToDto()).ToList();
    }

    public async Task<SubmissionDto> GetByIdAsync(Guid submissionId, CancellationToken ct = default)
    {
        var submission = await _uow.Submissions.GetDetailedAsync(submissionId, ct)
                         ?? throw new NotFoundException("Submission", submissionId);

        // Owner student or the assignment's teacher (or admin) may view.
        if (_currentUser.Role == UserRole.Student && submission.StudentId != _currentUser.Id)
            throw new ForbiddenException("You can only view your own submission.");
        if (_currentUser.Role == UserRole.Teacher && submission.Assignment.TeacherId != _currentUser.Id)
            throw new ForbiddenException("You can only view submissions for your own assignments.");

        return submission.ToDto();
    }

    public async Task<SubmissionDto> GradeAsync(Guid submissionId, GradeSubmissionRequest request, CancellationToken ct = default)
    {
        var submission = await _uow.Submissions.GetDetailedAsync(submissionId, ct)
                         ?? throw new NotFoundException("Submission", submissionId);
        EnsureTeacherOwnsAssignment(submission.Assignment);

        if (request.Marks < 0 || request.Marks > submission.Assignment.MaxMarks)
            throw new BusinessRuleException($"Marks must be between 0 and {submission.Assignment.MaxMarks}.");

        submission.Marks = request.Marks;
        submission.Feedback = request.Feedback?.Trim();
        submission.Status = SubmissionStatus.Graded;
        submission.GradedAt = DateTime.UtcNow;
        submission.GradedById = _currentUser.Id;
        submission.UpdatedAt = DateTime.UtcNow;

        _uow.Submissions.Update(submission);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Teacher {TeacherId} graded submission {SubmissionId} ({Marks})",
            _currentUser.Id, submissionId, request.Marks);

        return (await _uow.Submissions.GetDetailedAsync(submissionId, ct))!.ToDto();
    }

    public async Task<SubmissionDto> ChangeStatusAsync(Guid submissionId, UpdateSubmissionStatusRequest request, CancellationToken ct = default)
    {
        var submission = await _uow.Submissions.GetDetailedAsync(submissionId, ct)
                         ?? throw new NotFoundException("Submission", submissionId);
        EnsureTeacherOwnsAssignment(submission.Assignment);

        submission.Status = request.Status;
        // Returning a submission for revision clears the graded metadata.
        if (request.Status is SubmissionStatus.Returned or SubmissionStatus.Submitted or SubmissionStatus.Late)
        {
            submission.GradedAt = null;
            submission.GradedById = null;
        }
        submission.UpdatedAt = DateTime.UtcNow;

        _uow.Submissions.Update(submission);
        await _uow.SaveChangesAsync(ct);
        return (await _uow.Submissions.GetDetailedAsync(submissionId, ct))!.ToDto();
    }

    // ---------------- Helpers ----------------

    private async Task<User> GetEnrolledStudentAsync(CancellationToken ct)
    {
        var student = await _uow.Users.GetByIdAsync(_currentUser.Id, ct)
                      ?? throw new NotFoundException("User", _currentUser.Id);
        if (student.ClassCourseId is null)
            throw new BusinessRuleException("You are not enrolled in any class/course.");
        return student;
    }

    private static void EnsureStudentCanAccess(Assignment assignment, User student)
    {
        if (!assignment.IsPublished || assignment.ClassCourseId != student.ClassCourseId)
            throw new ForbiddenException("This assignment is not available to you.");
    }

    private void EnsureTeacherOwnsAssignment(Assignment assignment)
    {
        if (_currentUser.Role == UserRole.Admin) return;
        if (assignment.TeacherId != _currentUser.Id)
            throw new ForbiddenException("You can only manage submissions for your own assignments.");
    }
}
