using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Application.Abstractions.Security;

/// <summary>Generates signed JWT access tokens for authenticated users.</summary>
public interface IJwtTokenGenerator
{
    (string Token, DateTime ExpiresAtUtc) GenerateToken(User user);
}
