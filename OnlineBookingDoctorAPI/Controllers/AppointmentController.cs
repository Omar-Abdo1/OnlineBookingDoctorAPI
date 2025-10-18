using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Appointment;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;
using OnlineBookingDoctorAPI.Helpers;

    namespace OnlineBookingAPI.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        [ServiceFilter(typeof(AuthorizeV1Filter))]
        public class AppointmentController : ControllerBase
        {
            public enum BookingStatus
            {
                Success,
                DoctorOrServiceNotFound,
                SlotAlreadyBooked, // The race condition failure
                ScheduleMismatch, // The chosen slot is not valid for the doctor/service
                ProfileNotFound, // Patient profile hasn't been created yet
                DatabaseError
            }
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IAppointmentService _appointmentService;

            public AppointmentController(IUnitOfWork unitOfWork, IMapper mapper, IAppointmentService appointmentService)
            {
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _appointmentService = appointmentService;
            }

            [HttpPost]
            [Authorize(Roles = "Patient")]
            public async Task<IActionResult> BookAppointment(AppointmentBookingDTO bookingDto)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var (status, appointmentId) = await _appointmentService.BookAppointmentAsync(userId, bookingDto);

                return status switch
                {
                    (int)BookingStatus.Success => CreatedAtAction(nameof(GetAppointmentDetail), new { id = appointmentId }, new { AppointmentId = appointmentId }),
                    (int)BookingStatus.ProfileNotFound => NotFound(new ApiErrorResponse(404, "Patient profile not found. Complete your profile first.")),
                    (int)BookingStatus.SlotAlreadyBooked => Conflict(new ApiErrorResponse(409, "The selected time slot has just been booked. Please try another time.")),
                    (int)BookingStatus.ScheduleMismatch => BadRequest(new ApiErrorResponse(400, "The selected time slot is invalid for the doctor/service.")),
                    _ => StatusCode(500, new ApiErrorResponse(500, "Could not process booking due to a server error."))
                };
            }

            [HttpGet("{Appointmentid:int}")]
            [Authorize(Roles = "Patient,Doctor")]
            public async Task<IActionResult> GetAppointmentDetail(int Appointmentid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var (details, isOwner) = await _appointmentService.GetAppointmentDetailsAsync(userId, Appointmentid);

                if (details == null && isOwner == false)
                    return NotFound(new ApiErrorResponse(404, "Appointment not found or access denied."));

                return Ok(details);
            }

            [HttpPut("{Appointmentid:int}/cancel")] // PUT /api/appointments/{id}/cancel
            [Authorize(Roles = "Patient")]
            public async Task<IActionResult> CancelAppointment(int Appointmentid)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var (success, isOwner) = await _appointmentService.CancelAppointmentAsync(Appointmentid, userId);

                if (!isOwner) return Forbid(); // 403 Forbidden: User is not the patient on this booking
                if (!success) return BadRequest(new ApiErrorResponse(400, "Could not cancel appointment (may be too late)."));

                return NoContent(); // 204 No Content
            }

    [HttpPut("{Appointmentid:int}/confirm")] // PUT /api/appointments/{id}/confirm
    [Authorize(Roles = "Doctor")] 
    public async Task<IActionResult> ConfirmAppointment(int Appointmentid)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 
        
        var (success, isDoctor) = await _appointmentService.ConfirmAppointmentAsync(Appointmentid, userId);

        if (!isDoctor) return Forbid(); // 403 Forbidden: User is not the assigned doctor
        if (!success) return BadRequest(new ApiErrorResponse(400, "Could not confirm appointment."));

        return NoContent(); // 204 No Content
    }



        }

    }
