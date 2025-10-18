using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore.DTO.Schedule;
using OnlineBookingCore.Enums;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    [ServiceFilter(typeof(AuthorizeV1Filter))]
    public class ScheduleController : ControllerBase
    {
        private readonly IServiceManagementService _serviceManagementService;

        public ScheduleController(IServiceManagementService serviceManagementService)
        {
            _serviceManagementService = serviceManagementService;
        }



    [HttpPost("schedules")]
    public async Task<IActionResult> CreateSchedule(ScheduleCreationDTO dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var (status, scheduleId) = await _serviceManagementService.CreateScheduleAsync(userId, dto);
        
        if (status == ScheduleManagementStatus.Success)
        {
            return Created(string.Empty, new { ScheduleId = scheduleId, Message = "Schedule block created successfully." });
        }
        return BadRequest(new ApiErrorResponse(400, "Failed to create schedule. Check service IDs or profile status."));
    }

        [HttpDelete("schedules/{ScheduleId:int}")]
        public async Task<IActionResult> DeleteSchedule(int ScheduleId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var status = await _serviceManagementService.DeleteScheduleAsync(ScheduleId, userId);

            return status switch
            {
                ScheduleManagementStatus.Success => NoContent(),
                ScheduleManagementStatus.ScheduleNotFound => NotFound(new ApiErrorResponse(404, "Schedule not found.")),
                ScheduleManagementStatus.NotOwner => BadRequest(new ApiErrorResponse(400, "Access denied. Not the schedule owner.")),
                ScheduleManagementStatus.ThereAreAppointmentBooked => BadRequest(new ApiErrorResponse(400, "Cannot delete schedule: Future appointments exist.")),
                _ => StatusCode(500, new ApiErrorResponse(500, "Deletion failed."))
            };
        }

        [HttpPut("schedules/{ScheduleId:int}")]
        public async Task<IActionResult> UpdateSchedule(int ScheduleId, ScheduleCreationDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var status = await _serviceManagementService.UpdateScheduleAsync(ScheduleId, userId, dto);

            return status switch
            {
                ScheduleManagementStatus.Success => NoContent(), // 204 No Content
                ScheduleManagementStatus.ScheduleNotFound => NotFound(new ApiErrorResponse(404, "Schedule not found.")),
                ScheduleManagementStatus.NotOwner => BadRequest(new ApiErrorResponse(400, "Access denied. Not the schedule owner.")),
                ScheduleManagementStatus.ThereAreAppointmentBooked => BadRequest(new ApiErrorResponse(400, "Update failed: Future appointments prevent modifications.")),
                _ => StatusCode(500, new ApiErrorResponse(500, "Update failed."))
            };
        }

}
}
