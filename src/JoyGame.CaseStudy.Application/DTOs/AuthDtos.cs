namespace JoyGame.CaseStudy.Application.DTOs;

public record LoginRequestDto(string Username, string Password);

public record AuthResponseDto
{
    public string Token { get; init; }
    public string RefreshToken { get; init; }
    public DateTime ExpiresAt { get; init; }
    public UserDto User { get; init; }
}

public record ResetPasswordDto
{
    public string Email { get; init; }
    public string Token { get; init; }
    public string NewPassword { get; init; }
}