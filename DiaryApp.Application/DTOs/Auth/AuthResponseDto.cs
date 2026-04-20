namespace DiaryApp.Application.DTOs.Auth;

public class AuthResponseDto
{
    public required string Token { get; set; }
    public required string UserId { get; set; }
    public required string Name { get; set; }
    public string? AvatarUrl { get; set; }
}