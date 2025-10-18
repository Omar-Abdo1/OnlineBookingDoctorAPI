using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingAPI.Helpers;
using StackExchange.Redis;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    [ServiceFilter(typeof(AuthorizeV1Filter))]
    public class ServiceController : ControllerBase
    {
    }
}
