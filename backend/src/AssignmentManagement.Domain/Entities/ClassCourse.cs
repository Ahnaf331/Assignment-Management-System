using AssignmentManagement.Domain.Common;

namespace AssignmentManagement.Domain.Entities;

/// <summary>
/// A class or course (e.g. "Grade 10 - Section A" or "CS101"). Students are enrolled
/// into a class; subjects and assignments hang off a class.
/// </summary>
public class ClassCourse : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    // Navigation
    public ICollection<Subject> Subjects { get; set; } = new List<Subject>();
    public ICollection<User> Students { get; set; } = new List<User>();
    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
