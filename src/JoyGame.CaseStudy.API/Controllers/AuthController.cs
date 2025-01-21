using JoyGame.CaseStudy.API.Extensions;
using JoyGame.CaseStudy.API.Models;
using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
using JoyGame.CaseStudy.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace JoyGame.CaseStudy.API.Controllers;

public class AuthController(
    IAuthService authService,
    ILogger<AuthController> logger)
    : BaseApiController
{
    private readonly IAuthService _authService = authService;
    private readonly ILogger<AuthController> _logger = logger;

    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var loginOperationResult = await _authService.LoginAsync(request);
        return HandleResult(loginOperationResult.ToApiResponse());
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        var forgotPasswordOperationResult = await _authService.ForgotPasswordAsync(email);

        return HandleResult(forgotPasswordOperationResult.ToApiResponse());
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        var resetPasswordOperationResult = await _authService.ResetPasswordAsync(request);
        return HandleResult(resetPasswordOperationResult.ToApiResponse());
    }
}