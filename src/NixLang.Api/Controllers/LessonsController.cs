using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Common.Models;
using NixLang.Application.Lessons.Queries.GetLessons;
using NixLang.Application.Lessons.Queries.GetLessonById;

using NixLang.Application.Lessons.Commands.ToggleFavorite;
using NixLang.Application.Lessons.Queries.GetUserProgress;
using NixLang.Application.Lessons.Commands.SaveLessonProgress;

namespace NixLang.Api.Controllers;

public record SaveProgressRequestBody(decimal ProgressPercentage, string Status, List<SaveExerciseResultDto> Results);

[ApiController]
[Route("api/lessons")]
[Authorize]
public class LessonsController : ControllerBase
{
    private readonly ISender _mediator;

    public LessonsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<LessonSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLessons(
        [FromQuery] int page = 1, 
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? level = null,
        [FromQuery] string? categoryIds = null)
    {
        List<Guid>? parsedCategoryIds = null;
        if (!string.IsNullOrWhiteSpace(categoryIds))
        {
            parsedCategoryIds = categoryIds
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(Guid.Parse)
                .ToList();
        }

        var response = await _mediator.Send(new GetLessonsQuery(page, pageSize, search, level, parsedCategoryIds));
        return Ok(response);
    }

    [HttpGet("progress")]
    [ProducesResponseType(typeof(UserProgressResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserProgress()
    {
        var response = await _mediator.Send(new GetUserProgressQuery());
        return Ok(response);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LessonDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var response = await _mediator.Send(new GetLessonByIdQuery(id));
        return Ok(response);
    }

    [HttpPost("{id}/favorite")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFavorite(Guid id)
    {
        var isFavorite = await _mediator.Send(new ToggleFavoriteCommand(id));
        return Ok(new { isFavorite });
    }

    [HttpPost("{id}/progress")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveLessonProgress(Guid id, [FromBody] SaveProgressRequestBody body)
    {
        var command = new SaveLessonProgressCommand(id, body.ProgressPercentage, body.Status, body.Results);
        var success = await _mediator.Send(command);
        return Ok(new { success });
    }
}
