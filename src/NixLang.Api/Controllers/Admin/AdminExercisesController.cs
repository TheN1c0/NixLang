using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Common.Models;
using NixLang.Application.Exercises.Commands.CreateExercise;
using NixLang.Application.Exercises.Commands.DeleteExercise;
using NixLang.Application.Exercises.Commands.UpdateExercise;
using NixLang.Application.Exercises.Queries.GetExerciseById;
using NixLang.Application.Exercises.Queries.GetExercises;

namespace NixLang.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/exercises")]
[Authorize(Roles = "Administrator,Admin")]
public class AdminExercisesController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminExercisesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ExerciseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetExercises(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var response = await _mediator.Send(new GetExercisesQuery(page, pageSize, search));
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExerciseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _mediator.Send(new GetExerciseByIdQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateExerciseCommand command)
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
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExerciseCommand command)
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteExerciseCommand(id));
        return Ok(new { success });
    }
}
