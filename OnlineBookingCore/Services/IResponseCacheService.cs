using System;

namespace OnlineBookingCore.Services;

public interface IResponseCacheService
{
    Task CacheResponseAsync(string CacheKey , object Response ,TimeSpan expiration);
    Task<string?>GetCachedResponseAsync(string CacheKey);
}
