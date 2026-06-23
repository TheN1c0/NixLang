using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Common.Models;
using NixLang.Application.Lessons.Queries.GetLessons;

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
    public async Task<IActionResult> GetLessons([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _mediator.Send(new GetLessonsQuery(page, pageSize));
        return Ok(response);
    }
}
