using AssignmentManagement.Application.DTOs.Assignments;
using AssignmentManagement.Application.Services;
using AssignmentManagement.Domain.Enums;
using AssignmentManagement.Domain.Exceptions;
using AssignmentManagement.Tests.Common;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace AssignmentManagement.Tests;

public class AssignmentServiceTests
{
    private static AssignmentService CreateSut(TestHarness h) =>
        new(h.Uow, h.CurrentUser, NullLogger<AssignmentService>.Instance);

    private static CreateAssignmentRequest ValidRequest(TestHarness h, bool publish = true) =>
        new("Homework 1", "Do it", DateTime.UtcNow.AddDays(5), 50,
            h.ClassId, h.MathSubjectId, AllowResubmission: true, AllowLateSubmission: false, PublishImmediately: publish);

    [Fact]
    public async Task Create_ForAssignedSubject_Succeeds()
    {
        using var h = new TestHarness();
        h.AsTeacher();
        var sut = CreateSut(h);

        var result = await sut.CreateAsync(ValidRequest(h));

        result.Title.Should().Be("Homework 1");
        result.Status.Should().Be(AssignmentStatus.Published);
        result.TeacherId.Should().Be(h.TeacherId);
    }

    [Fact]
    public async Task Create_ForUnassignedSubject_ThrowsForbidden()
    {
        using var h = new TestHarness();
        h.AsTeacher(); // Teacher One is NOT assigned to the CS subject/class.
        var sut = CreateSut(h);

        var request = ValidRequest(h) with { ClassCourseId = h.OtherClassId, SubjectId = h.CsSubjectId };
        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Create_WithSubjectNotInClass_ThrowsBusinessRule()
    {
        using var h = new TestHarness();
        h.AsTeacher();
        var sut = CreateSut(h);

        // Math subject belongs to ClassId, not OtherClassId.
        var request = ValidRequest(h) with { ClassCourseId = h.OtherClassId };
        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Create_WithPastDeadline_ThrowsBusinessRule()
    {
        using var h = new TestHarness();
        h.AsTeacher();
        var sut = CreateSut(h);

        var request = ValidRequest(h) with { Deadline = DateTime.UtcNow.AddDays(-1) };
        var act = () => sut.CreateAsync(request);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }

    [Fact]
    public async Task Update_ByNonOwnerTeacher_ThrowsForbidden()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published);
        h.AsOtherTeacher();
        var sut = CreateSut(h);

        var act = () => sut.UpdateAsync(assignment.Id,
            new UpdateAssignmentRequest("x", "y", DateTime.UtcNow.AddDays(2), 10, true, false));

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task StudentList_ReturnsOnlyPublishedForOwnClass()
    {
        using var h = new TestHarness();
        h.AddAssignment(AssignmentStatus.Published);
        h.AddAssignment(AssignmentStatus.Draft); // must be hidden from students
        h.AsStudent();
        var sut = CreateSut(h);

        var result = await sut.ListForStudentAsync(subjectId: null, search: null);

        result.Should().HaveCount(1);
        result[0].SubjectName.Should().Be("Mathematics");
    }

    [Fact]
    public async Task StudentFromAnotherClass_SeesNoAssignments()
    {
        using var h = new TestHarness();
        h.AddAssignment(AssignmentStatus.Published); // belongs to ClassId
        h.AsOtherClassStudent();
        var sut = CreateSut(h);

        var result = await sut.ListForStudentAsync(subjectId: null, search: null);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task StudentGet_DraftAssignment_ThrowsForbidden()
    {
        using var h = new TestHarness();
        var draft = h.AddAssignment(AssignmentStatus.Draft);
        h.AsStudent();
        var sut = CreateSut(h);

        var act = () => sut.GetByIdAsync(draft.Id);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task Unpublish_WithExistingSubmissions_ThrowsBusinessRule()
    {
        using var h = new TestHarness();
        var assignment = h.AddAssignment(AssignmentStatus.Published);
        h.Db.Submissions.Add(new AssignmentManagement.Domain.Entities.Submission
        {
            AssignmentId = assignment.Id,
            StudentId = h.StudentId,
            Content = "answer",
            Status = SubmissionStatus.Submitted
        });
        await h.Db.SaveChangesAsync();
        h.Db.ChangeTracker.Clear();

        h.AsTeacher();
        var sut = CreateSut(h);

        var act = () => sut.UnpublishAsync(assignment.Id);

        await act.Should().ThrowAsync<BusinessRuleException>();
    }
}
