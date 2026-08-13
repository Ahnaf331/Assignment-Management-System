using AssignmentManagement.Domain.Common;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Domain.Entities;

/// <summary>
/// An application user. A single table holds Admin, Teacher and Student accounts,
/// distinguished by <see cref="Role"/>. Students are optionally linked to a class
/// via <see cref="ClassCourseId"/>.
/// </summary>
public class User : BaseEntity
{
    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>The class a student belongs to. Null for Admin/Teacher accounts.</summary>
    public Guid? ClassCourseId { get; set; }
    public ClassCourse? ClassCourse { get; set; }

    // Navigation
    public ICollection<TeacherAssignment> TeacherAssignments { get; set; } = new List<TeacherAssignment>();
    public ICollection<Assignment> CreatedAssignments { get; set; } = new List<Assignment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
