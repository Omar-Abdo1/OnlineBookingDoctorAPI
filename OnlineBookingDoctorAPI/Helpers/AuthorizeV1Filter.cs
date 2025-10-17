using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.Entities;
using OnlineBookingRespository.Data;

namespace OnlineBookingAPI.Helpers;

public class AuthorizeV1Filter :  IAsyncActionFilter
{

    private readonly OnlineBookingContext _dbContext;

    public AuthorizeV1Filter(OnlineBookingContext dbContext)
    {
        _dbContext = dbContext;
    }
   
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        var userClaimsPrincipal = context.HttpContext.User;
                
        if (!userClaimsPrincipal.Identity.IsAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        string userId = userClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        string tokenRevocationStamp = userClaimsPrincipal.FindFirstValue("SecurityStamp");

        var currentUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);

        if (currentUser == null || currentUser.SecurityStamp == null || 
            currentUser.SecurityStamp != tokenRevocationStamp)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        
        await next.Invoke();
    }
}
