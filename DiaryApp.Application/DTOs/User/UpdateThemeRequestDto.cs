using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.User;

public class UpdateThemeRequestDto
{
    public required string ThemeId { get; set; }
}