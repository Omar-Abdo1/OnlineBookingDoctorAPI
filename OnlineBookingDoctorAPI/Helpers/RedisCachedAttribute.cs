using System;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OnlineBookingCore.Services;

namespace OnlineBookingAPI.Helpers;

public class RedisCachedAttribute : Attribute, IAsyncActionFilter
{
private readonly int _expireTimeInSeconds;

    public RedisCachedAttribute(int ExpireTimeInSeconds)
    {
        _expireTimeInSeconds = ExpireTimeInSeconds;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var cacheService = context.HttpContext.RequestServices.GetRequiredService<IResponseCacheService>(); // ASK CLR explicitly for Injection
        var CacheKey = GenerateCachedKeyFromRequest(context.HttpContext.Request);
        var CachedResponse = await cacheService.GetCachedResponseAsync(CacheKey);
        if (!string.IsNullOrEmpty(CachedResponse)) // Return The CachedData
        {
            var contentResult = new ContentResult()
            {
                Content = CachedResponse,
                ContentType = "application/json",
                StatusCode = 200
            };
            context.Result = contentResult;
            return;
        }

        var AfterExecute =  await next.Invoke(); // run the next Attribute when it return back i will save the data and Cache it
        if (AfterExecute.Result is OkObjectResult SaveResponse)
        {
            await cacheService.CacheResponseAsync(CacheKey, SaveResponse.Value, TimeSpan.FromSeconds(_expireTimeInSeconds));
        }
    }

    private string GenerateCachedKeyFromRequest(HttpRequest request) // IMPORTANT
    {
        var KeyBuilder = new StringBuilder();
        KeyBuilder.Append(request.Path); // api/controller / .. etc  to know each endpoint i will cache
        var QueryParameters = request.Query.OrderBy(r=>r.Key); // so if he send the parameters Different but same values will be same CachedKey
        foreach (var (key,value) in QueryParameters)
        {
            // sort = name
            // page size = 1 
            // page index = 5
            
            // api/product | sort-name | pageSize-1 | pageIndex-5
            KeyBuilder.Append($"|{key}_{value}");
        }
        return KeyBuilder.ToString();
    }
}
