using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Paitent;
using OnlineBookingCore.Entities;
using OnlineBookingDoctorAPI.ErrorResponses;

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

        public PatientController(IUnitOfWork unitOfWork, IMapper mapper, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        [HttpPost("me/profile")] // Corrected route spelling
        public async Task<IActionResult> CreatePatientProfile(PaitentRegisterDTO paitentRegister)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. ROBUSTNESS CHECK: Prevent creation if profile already exists
            if (await _unitOfWork.Repository<Patient>().GetEntityByConditionAsync(p => p.UserId == userId) != null)
                return Conflict(new ApiErrorResponse(409, "Patient profile already exists for this user."));


            var newPatient = _mapper.Map<Patient>(paitentRegister);
            newPatient.UserId = userId; // Link the profile to the authenticated user

            await _unitOfWork.Repository<Patient>().AddAsync(newPatient);
            var res = await _unitOfWork.CompleteAsync();

            if (res <= 0)
                return BadRequest(new ApiErrorResponse(400, "Error while creating patient profile"));

            var profileDto = _mapper.Map<PaitentRegisterDTO>(newPatient);


            return CreatedAtAction(
                nameof(GetPatientProfile), // Method name as string
                new { fullname = profileDto.FullName }, // Anonymous object with PK for the route
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





    }
}
