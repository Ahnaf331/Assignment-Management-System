using AssignmentManagement.API.Authorization;
using AssignmentManagement.Application.Abstractions.Security;
using AssignmentManagement.Application.DTOs.TeacherAssignments;
using AssignmentManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/teacher-assignments")]
[Authorize]
public class TeacherAssignmentsController : ControllerBase
{
    private readonly ITeacherAssignmentService _service;
    private readonly ICurrentUser _currentUser;

    public TeacherAssignmentsController(ITeacherAssignmentService service, ICurrentUser currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    /// <summary>Admin: lists all teacher-subject-class assignments (optionally filtered by teacher).</summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> List([FromQuery] Guid? teacherId, CancellationToken ct)
        => Ok(await _service.ListAsync(teacherId, ct));

    /// <summary>Teacher: lists the subject/class pairs the current teacher is assigned to.</summary>
    [HttpGet("mine")]
    [Authorize(Roles = Roles.Teacher)]
    public async Task<ActionResult<IReadOnlyList<TeacherAssignmentDto>>> Mine(CancellationToken ct)
        => Ok(await _service.ListAsync(_currentUser.Id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<TeacherAssignmentDto>> Assign([FromBody] AssignTeacherRequest request, CancellationToken ct)
        => Ok(await _service.AssignAsync(request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Unassign(Guid id, CancellationToken ct)
    {
        await _service.UnassignAsync(id, ct);
        return NoContent();
    }
}
