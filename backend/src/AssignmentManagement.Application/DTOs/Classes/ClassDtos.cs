namespace AssignmentManagement.Application.DTOs.Classes;

public record ClassCourseDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    int StudentCount,
    int SubjectCount);

public record ClassCourseDetailDto(
    Guid Id,
    string Name,
    string Code,
    string? Description,
    IReadOnlyList<SubjectSummaryDto> Subjects);

public record SubjectSummaryDto(Guid Id, string Name, string Code);

public record CreateClassCourseRequest(string Name, string Code, string? Description);

public record UpdateClassCourseRequest(string Name, string Code, string? Description);
