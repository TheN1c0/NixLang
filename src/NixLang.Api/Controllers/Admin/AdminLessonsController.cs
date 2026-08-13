using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Common.Models;
using NixLang.Application.Lessons.Commands.CreateLesson;
using NixLang.Application.Lessons.Commands.DeleteLesson;
using NixLang.Application.Lessons.Commands.UpdateLesson;
using NixLang.Application.Lessons.Queries.GetAdminLessonById;
using NixLang.Application.Lessons.Queries.GetAdminLessons;

namespace NixLang.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/lessons")]
[Authorize(Roles = "Administrator,Admin")]
public class AdminLessonsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminLessonsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminLessonSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLessons([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _mediator.Send(new GetAdminLessonsQuery(page, pageSize));
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AdminLessonDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _mediator.Send(new GetAdminLessonByIdQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateLessonCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(null, null, new { id });
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Path ID and body ID mismatch.");
        }
        var success = await _mediator.Send(command);
        return Ok(new { success });
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteLessonCommand(id));
        return Ok(new { success });
    }
}
