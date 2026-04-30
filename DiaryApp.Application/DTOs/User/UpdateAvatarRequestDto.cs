using Microsoft.AspNetCore.Http;

namespace DiaryApp.Application.DTOs.User;

public class UploadAvatarRequestDto
{
    public IFormFile ImageFile { get; set; } = null!;
}