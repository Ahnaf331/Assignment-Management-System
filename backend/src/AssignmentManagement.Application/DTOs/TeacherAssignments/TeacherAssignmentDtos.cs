namespace AssignmentManagement.Application.DTOs.TeacherAssignments;

public record TeacherAssignmentDto(
    Guid Id,
    Guid TeacherId,
    string TeacherName,
    Guid SubjectId,
    string SubjectName,
    Guid ClassCourseId,
    string ClassCourseName);

public record AssignTeacherRequest(Guid TeacherId, Guid SubjectId, Guid ClassCourseId);
