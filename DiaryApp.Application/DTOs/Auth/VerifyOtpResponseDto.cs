using System.ComponentModel.DataAnnotations;

namespace DiaryApp.Application.DTOs.Auth;

public class VerifyOtpResponseDto
{
    public required string ResetToken { get; set; }
}