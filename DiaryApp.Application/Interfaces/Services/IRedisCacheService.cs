namespace DiaryApp.Application.Interfaces.Services;

public interface IRedisCacheService
{
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task<T?> GetAsync<T>(string key);
    Task RemoveAsync(string key);
}
