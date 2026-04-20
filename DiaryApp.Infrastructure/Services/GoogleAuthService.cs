using DiaryApp.Application.DTOs.Auth;
using DiaryApp.Application.Interfaces;
using DiaryApp.Infrastructure.Configurations;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace DiaryApp.Infrastructure.Providers;

public class GoogleAuthProvider(IOptions<GoogleSettings> googleSettings) : IGoogleAuthProvider
{
    private readonly GoogleSettings _googleSettings = googleSettings.Value;

    public async Task<GooglePayloadDto> ValidateTokenAsync(string idToken)
    {
        try
        {
            var validationSettings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new List<string> { _googleSettings.ClientId }
            };

            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, validationSettings);
            
            return new GooglePayloadDto
            {
                Email = payload.Email,
                Name = payload.Name,
                Picture = payload.Picture
            };
        }
        catch (InvalidJwtException)
        {
            throw new UnauthorizedAccessException("Lỗi xác thực Google: Token không hợp lệ.");
        }
        catch (Exception ex)
        {
            throw new Exception("Đã xảy ra lỗi khi xác thực Google: " + ex.Message);
        }
    }
}