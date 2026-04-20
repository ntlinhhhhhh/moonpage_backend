namespace DiaryApp.Infrastructure.Services;

using DiaryApp.Application.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDistributedCache _cache;
    public RedisCacheService(IDistributedCache cache) => _cache = cache;

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromHours(1) // Mặc định sống 1 tiếng
        };
        var jsonData = JsonSerializer.Serialize(value);
        await _cache.SetStringAsync(key, jsonData, options);
    }

    public async Task<T?> GetAsync<T>(string key)
    {
        var jsonData = await _cache.GetStringAsync(key);
        return jsonData == null ? default : JsonSerializer.Deserialize<T>(jsonData);
    }

    public async Task RemoveAsync(string key) => await _cache.RemoveAsync(key);
}
