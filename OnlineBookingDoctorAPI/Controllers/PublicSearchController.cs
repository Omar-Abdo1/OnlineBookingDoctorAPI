using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Review;
using OnlineBookingCore.DTO.Schedule;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;
using OnlineBookingDoctorAPI.Helpers;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/public")]
    [ApiController]
    public class PublicSearchController : ControllerBase
    {
        public enum SlotRetrievalStatus
        {
            Success,
            DoctorNotFound,
            ServiceNotFound,
            ServiceNotOffered,
            NoSlotsAvailable
        }

        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPublicSearchService _publicSearchService;
        private readonly IReviewService reviewService;

        public PublicSearchController(IUnitOfWork unitOfWork, IMapper mapper, IPublicSearchService publicSearchService,IReviewService reviewService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _publicSearchService = publicSearchService;
            this.reviewService = reviewService;
        }
        [HttpGet("departments")]
        public async Task<ActionResult> GetDepartments([FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 1, [FromQuery] int? DepartmentId = null)
        {
            
            var (count, departments) = await _publicSearchService.GetDepartmentsAsync(pageNumber.Value, pageSize.Value, DepartmentId);
            return Ok(new Pagination<DepartmentWithDoctorsDTO>(pageSize.Value, pageNumber.Value, count, departments));
        }

        [HttpGet("doctors")]
        public async Task<IActionResult> SearchDoctors([FromQuery] DoctorSearchQueryDTO query)
        {
            var (count, doctors) = await _publicSearchService.SearchDoctorsAsync(query);
            return Ok(new Pagination<FullDetailsDoctorDTO>(query.pageSize.Value, query.pageNumber.Value, count, doctors));
        }

        [HttpGet("doctors/{DoctorId:int}")]
        public async Task<IActionResult> GetDoctorByID(int DoctorId)
        {
            var doctor = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.Id == DoctorId);
            if (doctor is null)
                return NotFound(new ApiErrorResponse(404, $"Doctor with ID {DoctorId} not found."));
            var mappedDoctor = await _publicSearchService.GetDoctorAsync(DoctorId);
            return Ok(mappedDoctor);
        }



        // 4. GET /api/public/doctors/101/schedule?serviceId=...
        [HttpGet("doctors/{DoctorId:int}/schedule")]
        public async Task<IActionResult> GetDoctorSchedule(int DoctorId, [FromQuery] int serviceId, int? pageSize = 3, int? PageIndex = 1)
        {
            var (status, slots) = await _publicSearchService.GetAvailableSlotsAsync(DoctorId, serviceId);

            // Handle failure states based on the enum status
            if (status != (int)SlotRetrievalStatus.Success)
            {
                return status switch
                {
                    (int)SlotRetrievalStatus.DoctorNotFound => NotFound(new ApiErrorResponse(404, $"Doctor with ID {DoctorId} not found.")),
                    (int)SlotRetrievalStatus.ServiceNotFound => NotFound(new ApiErrorResponse(404, $"Service with ID {serviceId} not found.")),
                    (int)SlotRetrievalStatus.ServiceNotOffered => BadRequest(new ApiErrorResponse(400, $"Doctor {DoctorId} does not offer service {serviceId} during their scheduled times.")),
                    (int)SlotRetrievalStatus.NoSlotsAvailable => NotFound(new ApiErrorResponse(404, "No available slots in the next 7 days.")),
                    _ => StatusCode(500, new ApiErrorResponse(500, "An unknown error occurred."))
                };
            }

            // Handle success and pagination
            int count = slots.Count;
            var slotsPaged = slots.Skip((PageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();

            return Ok(new Pagination<AvailableSlotDTO>(pageSize.Value, PageIndex.Value, count, slotsPaged));
        }



    [HttpGet("doctors/{DoctorId:int}/reviews")] // GET /api/public/doctors/{id}/reviews
    public async Task<IActionResult> GetDoctorReviews(int DoctorId,int? pageSize=5,int?PageIndex=1)
    {
        if (DoctorId <= 0)
        {
            return BadRequest(new ApiErrorResponse(400, "Invalid Doctor ID."));
        }
        
        var reviews = await reviewService.GetDoctorReviewsAsync(DoctorId);

        if (reviews == null)
        {
            // The service returns null if the doctor ID was invalid
            return NotFound(new ApiErrorResponse(404, $"Doctor with ID {DoctorId} not found."));
        }

            if (reviews.Count == 0)
            {
                // Return 200 OK with an empty list if the doctor exists but has no reviews
                return Ok(new List<ReviewDetailsDTO>());
            }
            int count = reviews.Count;
           reviews = reviews.Skip((PageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
        return Ok(new Pagination<ReviewDetailsDTO>(pageSize.Value,PageIndex.Value,count,reviews) );
    }    
          
    }

}
