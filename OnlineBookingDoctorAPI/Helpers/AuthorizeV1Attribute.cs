using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.Entities;
using OnlineBookingRespository.Data;

namespace OnlineBookingAPI.Helpers;

public class AuthorizeV1Attribute : Attribute, IAsyncActionFilter
{
    private readonly ClaimsPrincipal user;

    public AuthorizeV1Attribute(ClaimsPrincipal User)
    {
        user = User;
    }

    

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {

        string userid = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var dbcontext = context.HttpContext.RequestServices.GetRequiredService<OnlineBookingContext>();
        var qry = await dbcontext.Users.Where(u => u.Id == userid).FirstOrDefaultAsync();


        

    }
}
