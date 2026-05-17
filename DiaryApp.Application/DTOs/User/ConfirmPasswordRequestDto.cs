namespace DiaryApp.Application.DTOs.User;

public class ConfirmPasswordRequestDto
{
    public string? Password { get; set; }
    public string? GoogleIdToken { get; set; }
}
