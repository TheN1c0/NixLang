using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NixLang.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestAuthController : ControllerBase
{
    [Authorize]
    [HttpGet("authenticated")]
    public IActionResult GetAuthenticated()
    {
        return Ok(new { Message = "Authenticated successfully", User = User.Identity?.Name });
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin")]
    public IActionResult GetAdminOnly()
    {
        return Ok(new { Message = "Admin authorized successfully" });
    }
}
