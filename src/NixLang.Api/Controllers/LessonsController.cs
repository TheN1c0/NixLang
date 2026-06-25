using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Common.Models;
using NixLang.Application.Lessons.Queries.GetLessons;
using NixLang.Application.Lessons.Queries.GetLessonById;

namespace NixLang.Api.Controllers;

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

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(LessonDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLessonById(Guid id)
    {
        var response = await _mediator.Send(new GetLessonByIdQuery(id));
        return Ok(response);
    }
}
