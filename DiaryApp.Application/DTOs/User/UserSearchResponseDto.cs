namespace DiaryApp.Application.DTOs.User;

public class UserSearchResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string Email { get; set; } = string.Empty;
}