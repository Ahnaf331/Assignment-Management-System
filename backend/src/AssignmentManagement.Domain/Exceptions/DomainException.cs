namespace AssignmentManagement.Domain.Exceptions;

/// <summary>
/// Base class for expected, business-rule related failures. The API translates these
/// into meaningful HTTP status codes via the global exception middleware.
/// </summary>
public abstract class DomainException : Exception
{
    protected DomainException(string message) : base(message) { }
}

/// <summary>A requested entity does not exist. Maps to HTTP 404.</summary>
public sealed class NotFoundException : DomainException
{
    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string entity, object key)
        : base($"{entity} with identifier '{key}' was not found.") { }
}

/// <summary>The caller is authenticated but not permitted to perform the action. Maps to HTTP 403.</summary>
public sealed class ForbiddenException : DomainException
{
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>A business rule was violated (e.g. submitting after the deadline). Maps to HTTP 409/422.</summary>
public sealed class BusinessRuleException : DomainException
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>A conflicting state was detected (e.g. duplicate email). Maps to HTTP 409.</summary>
public sealed class ConflictException : DomainException
{
    public ConflictException(string message) : base(message) { }
}
