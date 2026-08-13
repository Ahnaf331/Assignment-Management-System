using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Abstractions.Security;

/// <summary>
/// Abstraction over the authenticated caller, resolved from the JWT by the API layer.
/// Keeps the Application layer free of any HTTP dependency.
/// </summary>
public interface ICurrentUser
{
    Guid Id { get; }
    string Email { get; }
    UserRole Role { get; }
    bool IsAuthenticated { get; }
}
