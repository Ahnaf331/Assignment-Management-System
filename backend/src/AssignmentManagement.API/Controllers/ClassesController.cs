using AssignmentManagement.API.Authorization;
using AssignmentManagement.Application.DTOs.Classes;
using AssignmentManagement.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AssignmentManagement.API.Controllers;

[ApiController]
[Route("api/classes")]
[Authorize]
public class ClassesController : ControllerBase
{
    private readonly IClassCourseService _service;

    public ClassesController(IClassCourseService service) => _service = service;

    /// <summary>Lists classes/courses. Available to all authenticated users (used for dropdowns).</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ClassCourseDto>>> List([FromQuery] string? search, CancellationToken ct)
        => Ok(await _service.ListAsync(search, ct));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClassCourseDetailDto>> Get(Guid id, CancellationToken ct)
        => Ok(await _service.GetByIdAsync(id, ct));

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ClassCourseDto>> Create([FromBody] CreateClassCourseRequest request, CancellationToken ct)
    {
        var created = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<ActionResult<ClassCourseDto>> Update(Guid id, [FromBody] UpdateClassCourseRequest request, CancellationToken ct)
        => Ok(await _service.UpdateAsync(id, request, ct));

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteAsync(id, ct);
        return NoContent();
    }
}
