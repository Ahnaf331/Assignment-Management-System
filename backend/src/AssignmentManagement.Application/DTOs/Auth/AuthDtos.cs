using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.DTOs.Auth;

public record LoginRequest(string Email, string Password);

public record AuthResponse(
    string Token,
    DateTime ExpiresAtUtc,
    UserSummaryDto User);

public record UserSummaryDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    Guid? ClassCourseId,
    string? ClassCourseName);

/// <summary>Payload a signed-in user can use to change their own password.</summary>
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
