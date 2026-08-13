using AssignmentManagement.API.Authorization;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Application.DTOs.Assignments;
using AssignmentManagement.Application.DTOs.Submissions;
using AssignmentManagement.Application.Services.Interfaces;
using AssignmentManagement.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/assignments")]
[Authorize]
public class AssignmentsController : ControllerBase
{
    private readonly IAssignmentService _assignmentService;
    private readonly ISubmissionService _submissionService;
    private readonly ICurrentUser _currentUser;

    public AssignmentsController(
        IAssignmentService assignmentService,
        ISubmissionService submissionService,
        ICurrentUser currentUser)
    {
        _assignmentService = assignmentService;
        _submissionService = submissionService;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Lists assignments relevant to the caller: Admin sees all, a Teacher sees the
    /// assignments they authored, and a Student sees published assignments for their class.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] Guid? classCourseId, [FromQuery] Guid? subjectId,
        [FromQuery] AssignmentStatus? status, [FromQuery] string? search, CancellationToken ct)
    {
        return _currentUser.Role switch
        {
            UserRole.Admin => Ok(await _assignmentService.ListAllAsync(classCourseId, subjectId, status, search, ct)),
            UserRole.Teacher => Ok(await _assignmentService.ListForTeacherAsync(classCourseId, subjectId, status, search, ct)),
            _ => Ok(await _assignmentService.ListForStudentAsync(subjectId, search, ct))
        };
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssignmentDto>> Get(Guid id, CancellationToken ct)
        => Ok(await _assignmentService.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<AssignmentDto>> Create([FromBody] CreateAssignmentRequest request, CancellationToken ct)
    {
        var created = await _assignmentService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<AssignmentDto>> Update(Guid id, [FromBody] UpdateAssignmentRequest request, CancellationToken ct)
        => Ok(await _assignmentService.UpdateAsync(id, request, ct));

    [HttpPost("{id:guid}/publish")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<AssignmentDto>> Publish(Guid id, CancellationToken ct)
        => Ok(await _assignmentService.PublishAsync(id, ct));

    [HttpPost("{id:guid}/unpublish")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<AssignmentDto>> Unpublish(Guid id, CancellationToken ct)
        => Ok(await _assignmentService.UnpublishAsync(id, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.AdminOrTeacher)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _assignmentService.DeleteAsync(id, ct);
        return NoContent();
    }

    // -------- Submissions scoped to an assignment --------

    /// <summary>Teacher/Admin: lists all submissions for an assignment.</summary>
    [HttpGet("{assignmentId:guid}/submissions")]
    [Authorize(Roles = Roles.AdminOrTeacher)]
    public async Task<ActionResult<IReadOnlyList<SubmissionDto>>> ListSubmissions(Guid assignmentId, CancellationToken ct)
        => Ok(await _submissionService.ListForAssignmentAsync(assignmentId, ct));

    /// <summary>Student: submits an answer to an assignment.</summary>
    [HttpPost("{assignmentId:guid}/submissions")]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<SubmissionDto>> Submit(Guid assignmentId, [FromBody] CreateSubmissionRequest request, CancellationToken ct)
        => Ok(await _submissionService.SubmitAsync(assignmentId, request, ct));

    /// <summary>Student: updates their existing submission (before the deadline, if allowed).</summary>
    [HttpPut("{assignmentId:guid}/submissions")]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<SubmissionDto>> UpdateSubmission(Guid assignmentId, [FromBody] UpdateSubmissionRequest request, CancellationToken ct)
        => Ok(await _submissionService.UpdateOwnAsync(assignmentId, request, ct));

    /// <summary>Student: gets their own submission for an assignment (204 if none yet).</summary>
    [HttpGet("{assignmentId:guid}/submissions/mine")]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<SubmissionDto>> MySubmission(Guid assignmentId, CancellationToken ct)
    {
        var result = await _submissionService.GetOwnAsync(assignmentId, ct);
        return result is null ? NoContent() : Ok(result);
    }
}
