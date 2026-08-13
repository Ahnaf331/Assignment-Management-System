using AssignmentManagement.Domain.Common;

namespace AssignmentManagement.Domain.Entities;

/// <summary>
/// Junction that assigns a teacher to teach a given subject within a given class.
/// The Admin creates these; a teacher may only author assignments for subject/class
/// pairs they are assigned to.
/// </summary>
public class TeacherAssignment : BaseEntity
{
    public Guid TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;
}
