using AssignmentManagement.Application.Abstractions.Persistence;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Application.DTOs.Users;
using AssignmentManagement.Application.Mapping;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Entities;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace AssignmentManagement.Application.Services;

/// <summary>Admin-facing user management.</summary>
public class UserService : IUserService
{
    private readonly IUnitOfWork _uow;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUser _currentUser;
    private readonly ILogger<UserService> _logger;

    public UserService(IUnitOfWork uow, IPasswordHasher passwordHasher, ICurrentUser currentUser, ILogger<UserService> logger)
    {
        _uow = uow;
        _passwordHasher = passwordHasher;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<IReadOnlyList<UserDto>> ListAsync(UserRole? role, string? search, CancellationToken ct = default)
    {
        var users = await _uow.Users.ListAsync(role, search, ct);
        return users.Select(u => u.ToDto()).ToList();
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdWithClassAsync(id, ct)
                   ?? throw new NotFoundException("User", id);
        return user.ToDto();
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _uow.Users.EmailExistsAsync(email, null, ct))
            throw new ConflictException($"A user with email '{email}' already exists.");

        await ValidateClassAssignmentAsync(request.Role, request.ClassCourseId, ct);

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = request.Role,
            IsActive = true,
            ClassCourseId = request.Role == UserRole.Student ? request.ClassCourseId : null
        };

        await _uow.Users.AddAsync(user, ct);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Admin {AdminId} created user {UserId} ({Role})", _currentUser.Id, user.Id, user.Role);

        return (await _uow.Users.GetByIdWithClassAsync(user.Id, ct))!.ToDto();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default)
    {
        var user = await _uow.Users.GetByIdAsync(id, ct)
                   ?? throw new NotFoundException("User", id);

        await ValidateClassAssignmentAsync(request.Role, request.ClassCourseId, ct);

        user.FullName = request.FullName.Trim();
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.ClassCourseId = request.Role == UserRole.Student ? request.ClassCourseId : null;
        user.UpdatedAt = DateTime.UtcNow;

        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Admin {AdminId} updated user {UserId}", _currentUser.Id, user.Id);

        return (await _uow.Users.GetByIdWithClassAsync(user.Id, ct))!.ToDto();
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        if (id == _currentUser.Id)
            throw new BusinessRuleException("You cannot delete your own account.");

        var user = await _uow.Users.GetByIdAsync(id, ct)
                   ?? throw new NotFoundException("User", id);

        // Soft-delete to preserve historical assignments/submissions referencing the user.
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        _uow.Users.Update(user);
        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("Admin {AdminId} deactivated user {UserId}", _currentUser.Id, user.Id);
    }

    private async Task ValidateClassAssignmentAsync(UserRole role, Guid? classId, CancellationToken ct)
    {
        if (role != UserRole.Student) return;
        if (classId is null)
            throw new BusinessRuleException("A student must be assigned to a class/course.");
        if (await _uow.Classes.GetByIdAsync(classId.Value, ct) is null)
            throw new NotFoundException("ClassCourse", classId.Value);
    }
}
