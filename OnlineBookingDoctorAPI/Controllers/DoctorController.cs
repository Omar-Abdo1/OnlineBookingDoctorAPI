using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Service;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Enums;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;
using OnlineBookingDoctorAPI.Helpers;
using OnlineBookingRespository;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Doctor")]
    [ServiceFilter(typeof(AuthorizeV1Filter))]
    public class DoctorController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IMapper mapper;
        private readonly IServiceManagementService _serviceManagementService;
        private readonly DbContext _context;

        public DoctorController(IUnitOfWork unitOfWork, IMapper mapper,IServiceManagementService serviceManagementService)
        {
            this.unitOfWork = unitOfWork;
            this.mapper = mapper;
            _serviceManagementService = serviceManagementService;
            _context = unitOfWork.Context;
        }




        [HttpPost("me/profile")]
        public async Task<IActionResult> CreateDoctorProfile(DoctorProfileUpdateDTO profileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (await unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == userId) != null)
            {
                return Conflict(new ApiErrorResponse(409, "Doctor profile already exists for this user."));
            }

            var newDoctorProfile = mapper.Map<Doctor>(profileDto);

            newDoctorProfile.UserId = userId;

            newDoctorProfile.IsVerified = false;

            if (profileDto.DepartmentId.HasValue)
            {
                var departmentExists = await unitOfWork.Repository<Department>()
                    .GetEntityByConditionAsync(d => d.Id == profileDto.DepartmentId.Value);

                if (departmentExists == null)
                {
                    return BadRequest(new ApiErrorResponse(400, "Invalid Department ID provided."));
                }
            }

            await unitOfWork.Repository<Doctor>().AddAsync(newDoctorProfile);
            var res = await unitOfWork.CompleteAsync();

            if (res <= 0)
                return BadRequest(new ApiErrorResponse(400, "Error while creating doctor profile."));

            return CreatedAtAction(
                nameof(GetDoctorProfile),
                new { id = newDoctorProfile.Id },
                mapper.Map<DoctorProfileDetailsDTO>(newDoctorProfile));
        }

        [HttpGet("me/profile")]
        public async Task<IActionResult> GetDoctorProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doctor = await unitOfWork.Repository<Doctor>()
                .GetEntityByConditionAsync(d => d.UserId == userId,
                                           includes: [d => d.Department]);

            if (doctor == null)
                return NotFound(new ApiErrorResponse(404, "Doctor profile not found."));

            var doctorDTO = mapper.Map<DoctorProfileDetailsDTO>(doctor);

            return Ok(doctorDTO);
        }

        [HttpPut("me/profile")]
        public async Task<IActionResult> UpdateDoctorProfile(DoctorProfileUpdateDTO updatedProfileDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var existingDoctor = await unitOfWork.Repository<Doctor>()
                .GetEntityByConditionAsync(d => d.UserId == userId);

            if (existingDoctor == null)
                return NotFound(new ApiErrorResponse(404, "Doctor profile not found."));

            if (updatedProfileDto.DepartmentId.HasValue)
            {
                var departmentExists = await unitOfWork.Repository<Department>()
                    .GetEntityByConditionAsync(d => d.Id == updatedProfileDto.DepartmentId.Value);

                if (departmentExists == null)
                {
                    return BadRequest(new ApiErrorResponse(400, "Invalid Department ID provided."));
                }
            }

            mapper.Map(updatedProfileDto, existingDoctor);

            unitOfWork.Repository<Doctor>().Update(existingDoctor);

            var result = await unitOfWork.CompleteAsync();

            if (result <= 0)
            {
                return BadRequest(new ApiErrorResponse(400, "Error updating doctor profile or no changes were detected."));
            }

            return NoContent(); // 204 No Content
        }
      

    
    [HttpPost("/api/services")] // Overrides the Doctor base route
    public async Task<IActionResult> CreateService(ServiceCreationUpdateDTO dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var (status, serviceId) = await _serviceManagementService.CreateServiceAsync(userId, dto);
        
        if (status == ServiceManagementStatus.Success)
        {
            // Return 201 Created
            return Created(string.Empty, new { ServiceId = serviceId, Message = "Service created successfully." });
        }
        return BadRequest(new ApiErrorResponse(400, "Could not create service (Doctor profile missing or database error)."));
    }

    // /api/doctor/me/services
    [HttpGet("me/services")]
    public async Task<IActionResult> GetDoctorServices(int?pageSize=5,int?PageIndex=1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var services = await _serviceManagementService.GetDoctorServicesAsync(userId);
        int count = services.Count();
        services = services.Skip((PageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
        return Ok( new Pagination<ServiceDetailsDTO>(pageSize.Value,PageIndex.Value,count,services));
    }

    
    [HttpPut("/api/services/{id:int}")] // Overrides the Doctor base route
    public async Task<IActionResult> UpdateService(int id, ServiceCreationUpdateDTO dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var status = await _serviceManagementService.UpdateServiceAsync(id, userId, dto);

        return status switch
        {
            ServiceManagementStatus.Success => NoContent(),
            ServiceManagementStatus.ServiceNotFound => NotFound(new ApiErrorResponse(404, "Service not found.")),
            ServiceManagementStatus.NotOwner => BadRequest(new ApiErrorResponse(400, "Access denied. Not the service owner.")),
            _ => BadRequest(new ApiErrorResponse(400, "Update failed."))
        };
    }


    [HttpDelete("/api/services/{id}")] // Overrides the Doctor base route
    public async Task<IActionResult> DeleteService(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var status = await _serviceManagementService.DeleteServiceAsync(id, userId);

        return status switch
        {
            ServiceManagementStatus.Success => NoContent(),
            ServiceManagementStatus.ServiceNotFound => NotFound(new ApiErrorResponse(404, "Service not found.")),
            ServiceManagementStatus.NotOwner => BadRequest(new ApiErrorResponse(403, "Access denied. Not the service owner.")),
            _ => BadRequest(new ApiErrorResponse(400, "Deletion failed."))
        };
    } 




    }

    
}
