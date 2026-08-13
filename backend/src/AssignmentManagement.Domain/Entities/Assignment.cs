using AssignmentManagement.Domain.Common;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Domain.Entities;

/// <summary>
/// An assignment authored by a teacher for a specific class + subject.
/// </summary>
public class Assignment : BaseEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public DateTime Deadline { get; set; }

    public int MaxMarks { get; set; }

    public AssignmentStatus Status { get; set; } = AssignmentStatus.Draft;

    /// <summary>Whether students may update their submission before the deadline.</summary>
    public bool AllowResubmission { get; set; } = true;

    /// <summary>Whether students may submit after the deadline (flagged as Late).</summary>
    public bool AllowLateSubmission { get; set; } = false;

    public DateTime? PublishedAt { get; set; }

    public Guid ClassCourseId { get; set; }
    public ClassCourse ClassCourse { get; set; } = null!;

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    // Navigation
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public bool IsPublished => Status == AssignmentStatus.Published;
}
