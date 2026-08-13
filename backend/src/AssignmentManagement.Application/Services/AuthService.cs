using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Application.DTOs.Auth;
using AssignmentManagement.Application.Mapping;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AssignmentManagement.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUnitOfWork uow,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ICurrentUser currentUser,
        ILogger<AuthService> logger)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _uow.Users.GetByEmailAsync(email, ct);

        // Uniform error to avoid leaking which accounts exist.
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Email}", email);
            throw new BusinessRuleException("Invalid email or password.");
        }

        if (!user.IsActive)
            throw new ForbiddenException("This account has been deactivated. Contact an administrator.");

        var (token, expiresAt) = _tokenGenerator.GenerateToken(user);
        _logger.LogInformation("User {UserId} ({Role}) logged in", user.Id, user.Role);

        return new AuthResponse(token, expiresAt, user.ToSummaryDto());
    }

    public async Task<UserSummaryDto> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdWithClassAsync(_currentUser.Id, ct)
                   ?? throw new NotFoundException("User", _currentUser.Id);
        return user.ToSummaryDto();
    }

    public async Task ChangePasswordAsync(ChangePasswordRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(_currentUser.Id, ct)
                   ?? throw new NotFoundException("User", _currentUser.Id);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
            throw new BusinessRuleException("The current password is incorrect.");

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("User {UserId} changed their password", user.Id);
    }
}
