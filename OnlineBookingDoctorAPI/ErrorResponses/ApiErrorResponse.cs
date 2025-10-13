using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingDoctorAPI.ErrorResponses
{
    public class ApiErrorResponse
    {
        public int? StatusCode { get; set; }
        public string? Message { get; set; }

        public ApiErrorResponse(int? statusCode, string? message=null)
        {
            StatusCode = statusCode;
            Message = message ?? GetDefaultMessageForStatusCode(statusCode);
        }

        private string GetDefaultMessageForStatusCode(int? statusCode)
        {
            return statusCode switch
            {
                400 => "Bad Request",
                404 => "Not Found",
                500 => "Internal Server Error",
                 401 => "Unauthorized",
                 403 => "Forbidden",
                _ => "Unknown Error"
            };
        }
    }
}