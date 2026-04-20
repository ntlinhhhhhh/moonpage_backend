using DiaryApp.Application.DTOs.Auth;

namespace DiaryApp.Application.Interfaces;

public interface IGoogleAuthProvider
{
    Task<GooglePayloadDto> ValidateTokenAsync(string idToken);
}