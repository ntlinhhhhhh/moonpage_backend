using DiaryApp.Application.DTOs.Auth;
using DiaryApp.Application.Interfaces;
using DiaryApp.Infrastructure.Configurations;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace DiaryApp.Infrastructure.Providers;

public class GoogleAuthProvider(IOptions<GoogleCloudSettings> googleSettings) : IGoogleAuthProvider
{
    private readonly GoogleCloudSettings _googleSettings = googleSettings.Value;

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
            throw new UnauthorizedAccessException("Google authentication failed: Invalid token.");
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred during Google authentication: " + ex.Message);
        }
    }
}