namespace AssignmentManagement.Domain.Enums;

/// <summary>
/// Lifecycle of an assignment. Students can only see/submit to Published assignments.
/// </summary>
public enum AssignmentStatus
{
    Draft = 0,
    Published = 1
}
