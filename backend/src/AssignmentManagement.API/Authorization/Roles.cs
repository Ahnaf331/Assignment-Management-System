namespace AssignmentManagement.API.Authorization;

/// <summary>Role name constants used in [Authorize(Roles = ...)]. Must match <c>UserRole</c> names.</summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public const string AdminOrTeacher = "Admin,Teacher";
}
