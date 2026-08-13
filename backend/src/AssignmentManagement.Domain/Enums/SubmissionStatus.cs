namespace AssignmentManagement.Domain.Enums;

/// <summary>
/// Lifecycle of a student submission.
/// Submitted  -> on time, awaiting review.
/// Late       -> submitted after the deadline.
/// Graded     -> teacher has assigned marks/feedback.
/// Returned   -> teacher returned it to the student for revision.
/// </summary>
public enum SubmissionStatus
{
    Submitted = 0,
    Late = 1,
    Graded = 2,
    Returned = 3
}
