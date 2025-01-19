using JoyGame.CaseStudy.Application.Common;
using JoyGame.CaseStudy.Application.DTOs;
using JoyGame.CaseStudy.Application.Exceptions;
using JoyGame.CaseStudy.Application.Interfaces;
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
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return HandleResult(Result<AuthResponseDto>.Success(result));
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Login failed for user {Username}", request.Username);
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ForgotPassword([FromBody] string email)
    {
        try
        {
            await _authService.ForgotPasswordAsync(email);
            return HandleResult(Result<string>.Success("If the email exists, a password reset link will be sent."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password request for {Email}", email);
            return HandleResult(Result<string>.Success("If the email exists, a password reset link will be sent."));
        }
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request)
    {
        try
        {
            await _authService.ResetPasswordAsync(request);
            return HandleResult(Result<string>.Success("Password has been reset successfully"));
        }
        catch (BusinessRuleException ex)
        {
            return HandleResult(Result<object>.Failure(ex.Message));
        }
    }
}