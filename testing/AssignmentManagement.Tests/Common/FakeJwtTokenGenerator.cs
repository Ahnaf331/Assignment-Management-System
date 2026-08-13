using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Domain.Entities;

namespace AssignmentManagement.Tests.Common;

public class FakeJwtTokenGenerator : IJwtTokenGenerator
{
    public (string Token, DateTime ExpiresAtUtc) GenerateToken(User user)
        => ($"fake-token-for-{user.Id}", DateTime.UtcNow.AddHours(8));
}
