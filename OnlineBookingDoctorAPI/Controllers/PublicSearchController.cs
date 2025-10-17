using System.Linq.Expressions;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Schedule;
using OnlineBookingCore.Entities;
using OnlineBookingDoctorAPI.Helpers;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/public")]
    [ApiController]
    public class PublicSearchController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PublicSearchController(IUnitOfWork unitOfWork,IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }
        [HttpGet("departments")]
        public async Task<ActionResult> GetDepartments([FromQuery] int? pageNumber = 1, [FromQuery] int? pageSize = 1, [FromQuery] int? DepartmentId = null)
        {
            var departments = await _unitOfWork.Repository<Department>().GetAllByConditionAsync(
               pageSize: pageSize, pageIndex: pageNumber,
              criteria: d => !DepartmentId.HasValue || d.Id == DepartmentId.Value,
          orderBy: d => d.Id,
            includes: [d => d.Doctors]
            );
            int count = await _unitOfWork.Repository<Department>().CountAsync(
                criteria: d => !DepartmentId.HasValue || d.Id == DepartmentId.Value
                );

            var mappedDepartments = _mapper.Map<IReadOnlyList<DepartmentWithDoctorsDTO>>(departments);

            return Ok(new Pagination<DepartmentWithDoctorsDTO>(pageSize.Value, pageNumber.Value, count, mappedDepartments));
        }

    [HttpGet("doctors")]
    public async Task<IActionResult> SearchDoctors([FromQuery] DoctorSearchQueryDTO query)
    {
            Expression<Func<Doctor, bool>> criteria = d => true;
        if(query.DepartmentId.HasValue)
            criteria = criteria.And(d => d.DepartmentId == query.DepartmentId.Value);
        
        if(!string.IsNullOrEmpty(query.DoctorName))
            criteria = criteria.And(d => d.FullName.Contains(query.DoctorName));

            var doctors = await _unitOfWork.Context.Set<Doctor>().AsNoTracking()
            .Where(criteria)
            .OrderBy(d => d.Id)
            .Skip((query.pageNumber.Value - 1) * query.pageSize.Value)
            .Take(query.pageSize.Value).Select(d => new FullDetailsDoctorDTO
            {
                FullName = d.FullName,
                Address = d.Address,
                YearsOfExperience = d.YearsOfExperience,
                IsVerified = d.IsVerified,
                PhoneNumber = d.PhoneNumber,
                Department = new DepartmentDTO
                {
                    Name = d.Department.Name,
                    Description = d.Department.Description
                },
                AverageRating = d.Reviews.Average(r => r.Rating),
                ClinicsCount = d.Clinics.Count(),
                ReviewsCount = d.Reviews.Count(),
                Addresses = d.Clinics.Select(c => c.Address).Distinct().ToList()
            }).ToListAsync();
            int count= await _unitOfWork.Repository<Doctor>().CountAsync(criteria);
            return Ok(new Pagination<FullDetailsDoctorDTO>(query.pageSize.Value, query.pageNumber.Value, count, doctors));
    }

        // 4. GET /api/public/doctors/101/schedule?serviceId=...
        [HttpGet("doctors/{id}/schedule")]
        public async Task<IActionResult> GetDoctorSchedule(int id, [FromQuery] int serviceId,int?pageSize=3,int?PageIndex=1)
        {
            var slots = await GetAvailableSlotsAsync(id, serviceId);
            if (slots == null) return NotFound();
            int count = slots.Count();
            var slotsPaged = slots.Skip((PageIndex.Value - 1) * pageSize.Value).Take(pageSize.Value).ToList();
            return Ok(new Pagination<AvailableSlotDTO>(pageSize.Value, PageIndex.Value, count, slotsPaged)); 
        }
     
     [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<List<AvailableSlotDTO>> GetAvailableSlotsAsync(int doctorId, int serviceId, int daysAhead = 7)
{
    var availableSlots = new List<DateTime>();
    var today = DateTime.Today;
    
    // 1. Get Service Duration and Doctor Profiles
    // We assume Service and Doctor are loaded here, which gives us the key duration and existence check.
    // In a real scenario, check if the doctor exists and offers this service during any schedule block.
    
    // Retrieve the duration needed for slot generation
    var service = await _unitOfWork.Repository<Service>().GetEntityByConditionAsync(s=>s.Id==serviceId);
    if (service == null) return null; // Service doesn't exist
    var slotDuration = (int)service.DurationInMinutes; 

    // Retrieve the Doctor's associated schedules, including the DoctorScheduleService link
    var scheduleBlocks = await  _unitOfWork.Context.Set<DoctorSchedule>()
        .Where(ds => ds.DoctorId == doctorId && ds.IsAvailable)
        .Include(ds => ds.Services)
        .Where(ds => ds.Services.Any(s => s.Id == serviceId)) // Filter for schedules offering this Service
        .ToListAsync();

    // Retrieve all existing CONFIRMED and PENDING appointments for the doctor in the next 7 days
    var existingAppointments = await _unitOfWork.Context.Set<Appointment>()
        .Where(a => a.DoctorId == doctorId && a.StartTime.Date >= today && 
                   (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending))
        .ToListAsync();

            // --- 2. Iterate and Generate Slots ---

            for (int i = 0; i < daysAhead; i++)
            {
                var targetDate = today.AddDays(i);
                var dayOfWeek = targetDate.DayOfWeek.ToString();

                // Find the recurring schedule block for the current day
                var schedule = scheduleBlocks.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

                if (schedule == null) continue; // No schedule for this day

                // Start slot generation at the recurring schedule's StartTime
                var slotStart = targetDate.Date.Add(schedule.StartTime);
                var scheduleEnd = targetDate.Date.Add(schedule.EndTime);

                // Generate slots within the scheduled block
                while (slotStart.AddMinutes(slotDuration) <= scheduleEnd) // making the Appointments
                {
                    var slotEnd = slotStart.AddMinutes(slotDuration);

                    // --- 3. Overlap Check ---
                    // Check if this potential slot is consumed by an existing appointment
                    var isSlotBooked = existingAppointments.Any(a =>
                        // Check if the existing appointment starts within the current slot
                        (a.StartTime >= slotStart && a.StartTime < slotEnd) ||
                        // OR if the existing appointment overlaps the current slot
                        (a.StartTime < slotStart && a.StartTime.AddMinutes((double)a.BookedDurationMinutes) > slotStart)
                    );

                    if (!isSlotBooked)
                    {
                        availableSlots.Add(slotStart);
                    }

                    // Move to the next potential slot
                    slotStart = slotStart.AddMinutes(slotDuration);
                }
            }

            var slotDtos = availableSlots.Select(slotStart => new AvailableSlotDTO
        {
            SlotId = slotStart, // The unique identifier
            DurationMinutes = slotDuration,
            // Create a user-friendly string for display
            DisplayTime = slotStart.ToString("ddd, MMM dd, h:mm tt") 
        }).ToList();
            return slotDtos;    
}

}
}
