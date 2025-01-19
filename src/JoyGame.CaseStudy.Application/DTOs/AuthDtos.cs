namespace JoyGame.CaseStudy.Application.DTOs;

public record LoginRequestDto(string Username, string Password);

public record AuthResponseDto
{
    public string Token { get; init; }
    public UserDto User { get; init; }
}

public record ForgotPasswordResponseDto
{
    public required string ResetToken { get; init; }
    public required string Email { get; init; }
}

public record ResetPasswordDto
{
    public required string Email { get; init; }
    public required string ResetToken { get; init; }
    public required string NewPassword { get; init; }
}