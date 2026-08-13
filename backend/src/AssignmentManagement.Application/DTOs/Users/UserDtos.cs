using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.DTOs.Users;

public record UserDto(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    bool IsActive,
    Guid? ClassCourseId,
    string? ClassCourseName,
    DateTime CreatedAt);

public record CreateUserRequest(
    string FullName,
    string Email,
    string Password,
    UserRole Role,
    Guid? ClassCourseId);

public record UpdateUserRequest(
    string FullName,
    UserRole Role,
    bool IsActive,
    Guid? ClassCourseId);
