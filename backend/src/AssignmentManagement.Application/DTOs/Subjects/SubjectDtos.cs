namespace AssignmentManagement.Application.DTOs.Subjects;

public record SubjectDto(
    Guid Id,
    string Name,
    string Code,
    Guid ClassCourseId,
    string ClassCourseName);

public record CreateSubjectRequest(string Name, string Code, Guid ClassCourseId);

public record UpdateSubjectRequest(string Name, string Code);
