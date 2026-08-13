using AssignmentManagement.API.Authorization;
using AssignmentManagement.Application.DTOs.Submissions;
using AssignmentManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/submissions")]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _service;

    public SubmissionsController(ISubmissionService service) => _service = service;

    /// <summary>Student: lists all of the current student's submissions.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Student)]
    public async Task<ActionResult<IReadOnlyList<SubmissionDto>>> Mine(CancellationToken ct)
        => Ok(await _service.ListOwnAsync(ct));

    /// <summary>Returns a single submission (owner student, owning teacher, or admin).</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> Get(Guid id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    /// <summary>Teacher/Admin: assigns marks and feedback to a submission.</summary>
    [HttpPost("{id:guid}/grade")]
    [Authorize(Roles = Roles.AdminOrTeacher)]
    public async Task<ActionResult<SubmissionDto>> Grade(Guid id, [FromBody] GradeSubmissionRequest request, CancellationToken ct)
        => Ok(await _service.GradeAsync(id, request, ct));

    /// <summary>Teacher/Admin: changes the submission status (e.g. return for revision).</summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = Roles.AdminOrTeacher)]
    public async Task<ActionResult<SubmissionDto>> ChangeStatus(Guid id, [FromBody] UpdateSubmissionStatusRequest request, CancellationToken ct)
        => Ok(await _service.ChangeStatusAsync(id, request, ct));
}
