namespace AssignmentManagement.Domain.Enums;

/// <summary>
/// Application roles. Drives role-based authorization across the API.
/// </summary>
public enum UserRole
{
    Admin = 0,
    Teacher = 1,
    Student = 2
}
