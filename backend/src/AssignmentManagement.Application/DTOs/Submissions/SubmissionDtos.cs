using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.DTOs.Submissions;

public record SubmissionDto(
    Guid Id,
    Guid AssignmentId,
    string AssignmentTitle,
    int MaxMarks,
    Guid StudentId,
    string StudentName,
    string StudentEmail,
    string Content,
    DateTime SubmittedAt,
    DateTime? UpdatedAt,
    SubmissionStatus Status,
    int? Marks,
    string? Feedback,
    DateTime? GradedAt,
    string? GradedByName);

public record CreateSubmissionRequest(string Content);

public record UpdateSubmissionRequest(string Content);

public record GradeSubmissionRequest(int Marks, string? Feedback);

public record UpdateSubmissionStatusRequest(SubmissionStatus Status);
