using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NixLang.Application.Categories.Commands.CreateCategory;
using NixLang.Application.Categories.Commands.DeleteCategory;
using NixLang.Application.Categories.Commands.UpdateCategory;
using NixLang.Application.Categories.Queries.GetCategories;

namespace NixLang.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
[Authorize(Roles = "Administrator,Admin")]
public class AdminCategoriesController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminCategoriesController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<CategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCategories()
    {
        var response = await _mediator.Send(new GetCategoriesQuery());
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command)
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
        var success = await _mediator.Send(new DeleteCategoryCommand(id));
        return Ok(new { success });
    }
}
