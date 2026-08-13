using AssignmentManagement.Domain.Common;

namespace AssignmentManagement.Domain.Entities;

/// <summary>
/// A subject taught within a class/course (e.g. "Mathematics", "Physics").
/// </summary>
public class Subject : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public Guid ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;

    // Navigation
    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
    public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
}
