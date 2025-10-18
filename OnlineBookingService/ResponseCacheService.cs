using System.Text.Json;
using OnlineBookingCore.Services;
using StackExchange.Redis;

namespace OnlineBookingService;

public class ResponseCacheService : IResponseCacheService
{
     private readonly IDatabase _database;
    public ResponseCacheService(IConnectionMultiplexer  redis)
    {
        _database = redis.GetDatabase();
    }
    public async Task CacheResponseAsync(string CacheKey, object Response, TimeSpan expiration)
    {
        if(Response is null)return;
        var options = new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(Response, options);
        await _database.StringSetAsync(CacheKey, json, expiration);
    }

    public async Task<string?> GetCachedResponseAsync(string CacheKey)
    {
        var CachedResponse = await _database.StringGetAsync(CacheKey);
        if (CachedResponse.IsNullOrEmpty) return null;
        return CachedResponse;
    }
    
}