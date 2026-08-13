using AssignmentManagement.Application.DTOs.Assignments;
using AssignmentManagement.Application.DTOs.Auth;
using AssignmentManagement.Application.DTOs.Classes;
using AssignmentManagement.Application.DTOs.Subjects;
using AssignmentManagement.Application.DTOs.Submissions;
using AssignmentManagement.Application.DTOs.TeacherAssignments;
using AssignmentManagement.Application.DTOs.Users;
using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Application.Mapping;

/// <summary>Explicit entity → DTO mapping. Keeps the projection logic in one place.</summary>
public static class MappingExtensions
{
    public static UserSummaryDto ToSummaryDto(this User u) =>
        new(u.Id, u.FullName, u.Email, u.Role, u.ClassCourseId, u.ClassCourse?.Name);

    public static UserDto ToDto(this User u) =>
        new(u.Id, u.FullName, u.Email, u.Role, u.IsActive, u.ClassCourseId, u.ClassCourse?.Name, u.CreatedAt);

    public static ClassCourseDto ToDto(this ClassCourse c) =>
        new(c.Id, c.Name, c.Code, c.Description, c.Students?.Count ?? 0, c.Subjects?.Count ?? 0);

    public static ClassCourseDto ToDto(this ClassCourse c, int studentCount, int subjectCount) =>
        new(c.Id, c.Name, c.Code, c.Description, studentCount, subjectCount);

    public static ClassCourseDetailDto ToDetailDto(this ClassCourse c) =>
        new(c.Id, c.Name, c.Code, c.Description,
            (c.Subjects ?? new List<Subject>())
                .Select(s => new SubjectSummaryDto(s.Id, s.Name, s.Code)).ToList());

    public static SubjectDto ToDto(this Subject s) =>
        new(s.Id, s.Name, s.Code, s.ClassCourseId, s.ClassCourse?.Name ?? string.Empty);

    public static TeacherAssignmentDto ToDto(this TeacherAssignment t) =>
        new(t.Id, t.TeacherId, t.Teacher?.FullName ?? string.Empty,
            t.SubjectId, t.Subject?.Name ?? string.Empty,
            t.ClassCourseId, t.ClassCourse?.Name ?? string.Empty);

    public static AssignmentDto ToDto(this Assignment a) =>
        new(a.Id, a.Title, a.Description, a.Deadline, a.MaxMarks, a.Status,
            a.AllowResubmission, a.AllowLateSubmission,
            a.ClassCourseId, a.ClassCourse?.Name ?? string.Empty,
            a.SubjectId, a.Subject?.Name ?? string.Empty,
            a.TeacherId, a.Teacher?.FullName ?? string.Empty,
            a.CreatedAt, a.PublishedAt,
            a.Submissions?.Count ?? 0,
            DateTime.UtcNow > a.Deadline);

    public static StudentAssignmentDto ToStudentDto(this Assignment a, Submission? ownSubmission) =>
        new(a.Id, a.Title, a.Description, a.Deadline, a.MaxMarks,
            a.AllowResubmission, a.AllowLateSubmission,
            a.ClassCourse?.Name ?? string.Empty,
            a.Subject?.Name ?? string.Empty,
            a.Teacher?.FullName ?? string.Empty,
            DateTime.UtcNow > a.Deadline,
            ownSubmission is not null,
            ownSubmission?.Status,
            ownSubmission?.Marks);

    public static SubmissionDto ToDto(this Submission s) =>
        new(s.Id, s.AssignmentId, s.Assignment?.Title ?? string.Empty,
            s.Assignment?.MaxMarks ?? 0,
            s.StudentId, s.Student?.FullName ?? string.Empty, s.Student?.Email ?? string.Empty,
            s.Content, s.SubmittedAt, s.UpdatedAt, s.Status, s.Marks, s.Feedback,
            s.GradedAt, s.GradedBy?.FullName);
}
