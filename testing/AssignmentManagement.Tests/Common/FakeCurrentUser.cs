using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Tests.Common;

/// <summary>Test double for <see cref="ICurrentUser"/> that lets tests set the caller.</summary>
public class FakeCurrentUser : ICurrentUser
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsAuthenticated { get; set; } = true;

    public void SetUser(Guid id, UserRole role, string email = "user@test.com")
    {
        Id = id;
        Role = role;
        Email = email;
        IsAuthenticated = true;
    }
}
