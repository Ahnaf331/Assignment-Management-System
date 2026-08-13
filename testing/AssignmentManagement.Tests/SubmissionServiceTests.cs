using AssignmentManagement.Application.DTOs.Submissions;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Domain.Exceptions;
using AssignmentManagement.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace AssignmentManagement.Tests;

public class SubmissionServiceTests
{
    private static SubmissionService CreateSut(TestHarness h) =>
        new(h.Uow, h.CurrentUser, NullLogger<SubmissionService>.Instance);

    [Fact]
    public async Task Submit_ToPublishedAssignment_BeforeDeadline_Succeeds()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published, deadline: DateTime.UtcNow.AddDays(2));
        h.AsStudent();
        var sut = CreateSut(h);

        var result = await sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("My answer"));

        result.Status.Should().Be(SubmissionStatus.Submitted);
        result.Content.Should().Be("My answer");
    }

    [Fact]
    public async Task Submit_ToDraftAssignment_ThrowsForbidden()
    {
        using var h = new TestHarness();
        var draft = h.AddAssignment(AssignmentStatus.Draft);
        h.AsStudent();
        var sut = CreateSut(h);

        var act = () => sut.SubmitAsync(draft.Id, new CreateSubmissionRequest("My answer"));

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Submit_ByStudentFromAnotherClass_ThrowsForbidden()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published);
        h.AsOtherClassStudent();
        var sut = CreateSut(h);

        var act = () => sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("answer"));

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Submit_AfterDeadline_WhenLateNotAllowed_ThrowsBusinessRule()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published,
            deadline: DateTime.UtcNow.AddHours(-1), allowLateSubmission: false);
        h.AsStudent();
        var sut = CreateSut(h);

        var act = () => sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("late answer"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Submit_AfterDeadline_WhenLateAllowed_MarksAsLate()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published,
            deadline: DateTime.UtcNow.AddHours(-1), allowLateSubmission: true);
        h.AsStudent();
        var sut = CreateSut(h);

        var result = await sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("late answer"));

        result.Status.Should().Be(SubmissionStatus.Late);
    }

    [Fact]
    public async Task Submit_Twice_ThrowsConflict()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published);
        h.AsStudent();
        var sut = CreateSut(h);

        await sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("first"));
        var act = () => sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("second"));

        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Update_BeforeDeadline_WhenAllowed_Succeeds()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published,
            deadline: DateTime.UtcNow.AddDays(1), allowResubmission: true);
        h.AsStudent();
        var sut = CreateSut(h);

        await sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("first"));
        var result = await sut.UpdateOwnAsync(assignment.Id, new UpdateSubmissionRequest("updated answer"));

        result.Content.Should().Be("updated answer");
    }

    [Fact]
    public async Task Update_WhenResubmissionNotAllowed_ThrowsBusinessRule()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published,
            deadline: DateTime.UtcNow.AddDays(1), allowResubmission: false);
        h.AsStudent();
        var sut = CreateSut(h);

        await sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("first"));
        var act = () => sut.UpdateOwnAsync(assignment.Id, new UpdateSubmissionRequest("updated"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Update_AfterDeadline_ThrowsBusinessRule()
    {
        using var h = new TestHarness();
        // Allow late submission so the first submit succeeds after the deadline, then updating must fail.
        var assignment = h.AddAssignment(AssignmentStatus.Published,
            deadline: DateTime.UtcNow.AddHours(-1), allowResubmission: true, allowLateSubmission: true);
        h.AsStudent();
        var sut = CreateSut(h);

        await sut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("late"));
        var act = () => sut.UpdateOwnAsync(assignment.Id, new UpdateSubmissionRequest("changed"));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Grade_WithinMaxMarks_SetsGradedStatus()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published, maxMarks: 100);
        h.AsStudent();
        var studentSut = CreateSut(h);
        var submission = await studentSut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("answer"));

        h.AsTeacher();
        var teacherSut = CreateSut(h);
        var result = await teacherSut.GradeAsync(submission.Id, new GradeSubmissionRequest(85, "Well done"));

        result.Status.Should().Be(SubmissionStatus.Graded);
        result.Marks.Should().Be(85);
        result.Feedback.Should().Be("Well done");
    }

    [Fact]
    public async Task Grade_AboveMaxMarks_ThrowsBusinessRule()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published, maxMarks: 50);
        h.AsStudent();
        var studentSut = CreateSut(h);
        var submission = await studentSut.SubmitAsync(assignment.Id, new CreateSubmissionRequest("answer"));

        h.AsTeacher();
        var teacherSut = CreateSut(h);
        var act = () => teacherSut.GradeAsync(submission.Id, new GradeSubmissionRequest(75, null));

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Grade_ByNonOwnerTeacher_ThrowsForbidden()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published);
        h.AsStudent();
        var submission = await CreateSut(h).SubmitAsync(assignment.Id, new CreateSubmissionRequest("answer"));

        h.AsOtherTeacher();
        var sut = CreateSut(h);
        var act = () => sut.GradeAsync(submission.Id, new GradeSubmissionRequest(10, null));

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetById_ByAnotherStudent_ThrowsForbidden()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published);
        h.AsStudent();
        var submission = await CreateSut(h).SubmitAsync(assignment.Id, new CreateSubmissionRequest("answer"));

        h.AsOtherClassStudent();
        var sut = CreateSut(h);
        var act = () => sut.GetByIdAsync(submission.Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }
}
