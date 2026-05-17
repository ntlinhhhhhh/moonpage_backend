namespace DiaryApp.Application.DTOs.User;

public class ChangePasswordRequestDto
{
    public required string OldPassword { get; set; }
    public required string NewPassword { get; set; }
}
