using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Common.Models;
using NixLang.Application.EducationalContents.Queries.GetEducationalContentById;
using NixLang.Application.EducationalContents.Queries.GetEducationalContents;

namespace NixLang.Api.Controllers;

[ApiController]
[Route("api/content")]
[Authorize]
public class EducationalContentController : ControllerBase
{
    private readonly ISender _mediator;

    public EducationalContentController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EducationalContentItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetEducationalContents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] string? level = null)
    {
        var response = await _mediator.Send(new GetEducationalContentsQuery(page, pageSize, search, type, level));
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EducationalContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _mediator.Send(new GetEducationalContentByIdQuery(id));
        return Ok(response);
    }
}
