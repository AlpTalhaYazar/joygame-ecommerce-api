using JoyGame.CaseStudy.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace JoyGame.CaseStudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (!result.IsSuccess && result.Data == null)
            return NotFound(result);

        if (result.IsSuccess)
            return Ok(result);

        return BadRequest(result);
    }
}