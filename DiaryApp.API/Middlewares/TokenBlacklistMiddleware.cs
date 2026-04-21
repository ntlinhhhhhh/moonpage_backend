using DiaryApp.Application.Interfaces.Services;

namespace DiaryApp.API.Middlewares;
public class TokenBlacklistMiddleware(RequestDelegate next, IRedisCacheService cacheService)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
        if (!string.IsNullOrEmpty(token))
        {
            var isBlacklisted = await cacheService.GetAsync<bool>($"blacklist:{token}");
            if (isBlacklisted)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { message = "Token has been revoked. Please log in again." });
                return;
            }
        }
        await next(context);
    }
}