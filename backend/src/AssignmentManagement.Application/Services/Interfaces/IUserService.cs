using AssignmentManagement.Application.DTOs.Users;
using AssignmentManagement.Domain.Enums;

namespace AssignmentManagement.Application.Services.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> ListAsync(UserRole? role, string? search, CancellationToken ct = default);
    Task<UserDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
