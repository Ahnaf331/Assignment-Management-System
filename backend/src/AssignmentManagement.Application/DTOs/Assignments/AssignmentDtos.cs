using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.DTOs.Assignments;

public record AssignmentDto(
    Guid Id,
    string Title,
    string Description,
    DateTime Deadline,
    int MaxMarks,
    AssignmentStatus Status,
    bool AllowResubmission,
    bool AllowLateSubmission,
    Guid ClassCourseId,
    string ClassCourseName,
    Guid SubjectId,
    string SubjectName,
    Guid TeacherId,
    string TeacherName,
    DateTime CreatedAt,
    DateTime? PublishedAt,
    int SubmissionCount,
    bool IsOverdue);

/// <summary>Assignment as seen by a student, including the student's own submission state.</summary>
public record StudentAssignmentDto(
    Guid Id,
    string Title,
    string Description,
    DateTime Deadline,
    int MaxMarks,
    bool AllowResubmission,
    bool AllowLateSubmission,
    string ClassCourseName,
    string SubjectName,
    string TeacherName,
    bool IsOverdue,
    bool HasSubmitted,
    SubmissionStatus? SubmissionStatus,
    int? Marks);

public record CreateAssignmentRequest(
    string Title,
    string Description,
    DateTime Deadline,
    int MaxMarks,
    Guid ClassCourseId,
    Guid SubjectId,
    bool AllowResubmission,
    bool AllowLateSubmission,
    bool PublishImmediately);

public record UpdateAssignmentRequest(
    string Title,
    string Description,
    DateTime Deadline,
    int MaxMarks,
    bool AllowResubmission,
    bool AllowLateSubmission);
