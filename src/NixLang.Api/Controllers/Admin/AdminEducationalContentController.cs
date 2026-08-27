using System;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Common.Models;
using NixLang.Application.EducationalContents.Commands.CreateEducationalContent;
using NixLang.Application.EducationalContents.Commands.DeleteEducationalContent;
using NixLang.Application.EducationalContents.Commands.UpdateEducationalContent;
using NixLang.Application.EducationalContents.Queries.GetAdminEducationalContentById;
using NixLang.Application.EducationalContents.Queries.GetAdminEducationalContents;

namespace NixLang.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/content")]
[Authorize(Roles = "Administrator,Admin")]
public class AdminEducationalContentController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminEducationalContentController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<AdminEducationalContentSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetContents(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? type = null,
        [FromQuery] string? status = null)
    {
        var response = await _mediator.Send(new GetAdminEducationalContentsQuery(page, pageSize, search, type, status));
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AdminEducationalContentDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _mediator.Send(new GetAdminEducationalContentByIdQuery(id));
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create([FromBody] CreateEducationalContentCommand command)
    {
        var id = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEducationalContentCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest("Path ID and body ID mismatch.");
        }
        var success = await _mediator.Send(command);
        return Ok(new { success });
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _mediator.Send(new DeleteEducationalContentCommand(id));
        return Ok(new { success });
    }
}
