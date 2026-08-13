using AssignmentManagement.Application.DTOs.Auth;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Domain.Exceptions;
using AssignmentManagement.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace AssignmentManagement.Tests;

public class AuthServiceTests
{
    private static AuthService CreateSut(TestHarness h) =>
        new(h.Uow, h.Hasher, new FakeJwtTokenGenerator(), h.CurrentUser, NullLogger<AuthService>.Instance);

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokenAndUser()
    {
        using var h = new TestHarness();
        var sut = CreateSut(h);

        var result = await sut.LoginAsync(new LoginRequest("t1@test.com", "Teacher@123"));

        result.Token.Should().NotBeNullOrEmpty();
        result.User.Email.Should().Be("t1@test.com");
        result.User.Role.Should().Be(UserRole.Teacher);
    }

    [Fact]
    public async Task Login_IsCaseInsensitiveForEmail()
    {
        using var h = new TestHarness();
        var sut = CreateSut(h);

        var result = await sut.LoginAsync(new LoginRequest("T1@TEST.com", "Teacher@123"));

        result.User.Email.Should().Be("t1@test.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_Throws()
    {
        using var h = new TestHarness();
        var sut = CreateSut(h);

        var act = () => sut.LoginAsync(new LoginRequest("t1@test.com", "wrong-password"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Login_WithUnknownEmail_Throws()
    {
        using var h = new TestHarness();
        var sut = CreateSut(h);

        var act = () => sut.LoginAsync(new LoginRequest("nobody@test.com", "whatever"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Login_WithDeactivatedAccount_ThrowsForbidden()
    {
        using var h = new TestHarness();
        var user = await h.Uow.Users.GetByIdAsync(h.TeacherId);
        user!.IsActive = false;
        h.Uow.Users.Update(user);
        await h.Uow.SaveChangesAsync();

        var sut = CreateSut(h);
        var act = () => sut.LoginAsync(new LoginRequest("t1@test.com", "Teacher@123"));

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_Throws()
    {
        using var h = new TestHarness();
        h.AsTeacher();
        var sut = CreateSut(h);

        var act = () => sut.ChangePasswordAsync(new ChangePasswordRequest("nope", "NewPass@123"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task ChangePassword_WithValidCurrentPassword_UpdatesHash()
    {
        using var h = new TestHarness();
        h.AsTeacher();
        var sut = CreateSut(h);

        await sut.ChangePasswordAsync(new ChangePasswordRequest("Teacher@123", "NewPass@123"));

        // Old password no longer works; new one does.
        await new System.Func<Task>(() => sut.LoginAsync(new LoginRequest("t1@test.com", "Teacher@123")))
            .Should().ThrowAsync<BusinessRuleException>();
        var ok = await sut.LoginAsync(new LoginRequest("t1@test.com", "NewPass@123"));
        ok.Token.Should().NotBeNullOrEmpty();
    }
}
