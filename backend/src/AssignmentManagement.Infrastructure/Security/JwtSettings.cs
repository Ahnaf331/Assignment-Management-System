namespace AssignmentManagement.Infrastructure.Security;

/// <summary>Strongly-typed JWT configuration bound from appsettings / environment.</summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "AssignmentManagement";
    public string Audience { get; set; } = "AssignmentManagementClient";
    public int ExpiryMinutes { get; set; } = 480;
}
