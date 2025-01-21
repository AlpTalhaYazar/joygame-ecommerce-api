using JoyGame.CaseStudy.API.Models;
using JoyGame.CaseStudy.Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace JoyGame.CaseStudy.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(ApiResponse<T> result)
    {
        if (!result.Success && result.Data == null)
            return NotFound(result);

        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }
}