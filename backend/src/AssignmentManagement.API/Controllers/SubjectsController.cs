using AssignmentManagement.API.Authorization;
using AssignmentManagement.Application.DTOs.Subjects;
using AssignmentManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/subjects")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _service;

    public SubjectsController(ISubjectService service) => _service = service;

    /// <summary>Lists subjects, optionally filtered by class/course.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SubjectDto>>> List([FromQuery] Guid? classCourseId, CancellationToken ct)
        => Ok(await _service.ListAsync(classCourseId, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubjectDto>> Get(Guid id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SubjectDto>> Create([FromBody] CreateSubjectRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<SubjectDto>> Update(Guid id, [FromBody] UpdateSubjectRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
