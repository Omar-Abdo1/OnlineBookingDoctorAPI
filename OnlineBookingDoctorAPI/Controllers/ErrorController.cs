using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingDoctorAPI.ErrorResponses;

namespace OnlineBookingDoctorAPI.Controllers
{
    [ApiController]
    [Route("error/{code}")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public class ErrorController : ControllerBase
    {
        public ActionResult Error(int code)
        {
            return NotFound(new ApiErrorResponse(code,"The requested resource was not found."));
        }
    }
}