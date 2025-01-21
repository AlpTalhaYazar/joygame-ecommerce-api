using JoyGame.CaseStudy.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace JoyGame.CaseStudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(ApiResponse<T> result)
    {
        if (result.Success)
        {
            return Ok(result);
        }

        return result.Error.Code switch
        {
            int code when code is >= 1000 and < 2000 => BadRequest(result),
            int code when code is >= 2000 and < 3000 => Unauthorized(result),
            int code when code is >= 3000 and < 4000 => Forbid(result),
            int code when code is >= 4000 and < 5000 => NotFound(result),
            int code when code is >= 5000 and < 6000 => StatusCode(500, result),
            int code when code is >= 6000 and < 7000 => StatusCode(503, result),
            _ => StatusCode(500, result)
        };
    }

    private IActionResult Forbid<T>(ApiResponse<T> authenticationSchemes)
    {
        return StatusCode(403, authenticationSchemes);
    }
}