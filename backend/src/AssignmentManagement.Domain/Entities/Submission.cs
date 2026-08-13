using AssignmentManagement.Domain.Common;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Domain.Entities;

/// <summary>
/// A student's answer to an assignment. One submission per (assignment, student).
/// </summary>
public class Submission : BaseEntity
{
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    /// <summary>The student's answer text (could also be a link to uploaded work).</summary>
    public string Content { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Submitted;

    public int? Marks { get; set; }

    public string? Feedback { get; set; }

    public DateTime? GradedAt { get; set; }

    public Guid? GradedById { get; set; }
    public User? GradedBy { get; set; }
}
