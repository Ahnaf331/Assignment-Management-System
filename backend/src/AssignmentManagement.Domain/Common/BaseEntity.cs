namespace AssignmentManagement.Domain.Common;

/// <summary>
/// Base type for all persistent entities. Provides a surrogate key and audit timestamps.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
