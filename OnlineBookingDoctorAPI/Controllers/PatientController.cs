using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Appointment;
using OnlineBookingCore.DTO.Paitent;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;
using OnlineBookingDoctorAPI.Helpers;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Patient")]
    [ServiceFilter(typeof(AuthorizeV1Filter))]
    public class PatientController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly IAppointmentService _appointmentService;

        public PatientController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager,IAppointmentService appointmentService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
            _appointmentService = appointmentService;
        }

        [HttpPost("me/profile")]
        public async Task<IActionResult> CreatePatientProfile(PaitentRegisterDTO paitentRegister)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (await _unitOfWork.Repository<Patient>().GetEntityByConditionAsync(p => p.UserId == userId) != null)
                return Conflict(new ApiErrorResponse(409, "Patient profile already exists for this user."));


            var newPatient = _mapper.Map<Patient>(paitentRegister);
            newPatient.UserId = userId;

            await _unitOfWork.Repository<Patient>().AddAsync(newPatient);
            var res = await _unitOfWork.CompleteAsync();

            if (res <= 0)
                return BadRequest(new ApiErrorResponse(400, "Error while creating patient profile"));

            var profileDto = _mapper.Map<PaitentRegisterDTO>(newPatient);


            return CreatedAtAction(
                nameof(GetPatientProfile), // Method name as string
                new { fullname = profileDto.FullName },
                profileDto);
        }

        [HttpGet("me/profile")]
        public async Task<IActionResult> GetPatientProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var patient = await _unitOfWork.Repository<Patient>()
                .GetEntityByConditionAsync(p => p.UserId == userId);

            if (patient == null)
            {
                return NotFound(new ApiErrorResponse(404, "Patient profile not found. Please create it first."));
            }

            var patientDTO = _mapper.Map<PaitentRegisterDTO>(patient);

            return Ok(patientDTO);
        }

        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdatePatientProfile(PaitentRegisterDTO updatedProfileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingPatient = await _unitOfWork.Repository<Patient>()
                .GetEntityByConditionAsync(p => p.UserId == userId);

            if (existingPatient == null)
            {
                return NotFound(new ApiErrorResponse(404, "Patient profile not found. Please create it first."));
            }

            _mapper.Map(updatedProfileDto, existingPatient);

            _unitOfWork.Repository<Patient>().Update(existingPatient);

            var result = await _unitOfWork.CompleteAsync();

            if (result <= 0)
            {
                return BadRequest(new ApiErrorResponse(400, "Error updating patient profile."));
            }
            return Content("Updated Successfully");
        }

            [HttpGet("me/appointments")] // GET /api/patient/me/appointments
            [Authorize(Roles = "Patient")]
            public async Task<IActionResult> GetPatientAppointments([FromQuery] int? pageIndex = 1, [FromQuery] int? pageSize = 5)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var (count, appointments) = await _appointmentService.GetPatientAppointmentsAsync(userId, pageIndex, pageSize);

                return Ok(new Pagination<AppointmentSummaryDTO>(pageSize.Value, pageIndex.Value, count, appointments));
            }





    }
}
