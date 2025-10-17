using System;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Schedule;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Services;
using OnlineBookingRespository.Data;
using OnlineBookingService.Helpers;

namespace OnlineBookingService;

public class PublicSearchService : IPublicSearchService
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
    private readonly OnlineBookingContext _dbContext; // Direct access for complex includes
        private readonly IMapper _mapper;

    public PublicSearchService(IUnitOfWork unitOfWork, OnlineBookingContext dbContext,IMapper mapper)
    {
            _mapper = mapper;
        _unitOfWork = unitOfWork;
        _dbContext = dbContext;
    }

    

    public async Task<(int Status, IReadOnlyList<AvailableSlotDTO> Slots)> GetAvailableSlotsAsync(int doctorId, int serviceId, int daysAhead = 7)
    {
        var availableSlots = new List<DateTime>();
        var today = DateTime.Today;

        // --- 1. Validation and Data Retrieval ---

        // Check 1: Doctor is found
        var doctor = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.Id == doctorId);
        if (doctor == null)
            return ((int)SlotRetrievalStatus.DoctorNotFound, new List<AvailableSlotDTO>());

        // Check 2: Service is found
        var service = await _unitOfWork.Repository<Service>().GetEntityByConditionAsync(s=>s.Id==serviceId);
        if (service == null)
            return ((int)SlotRetrievalStatus.ServiceNotFound, new List<AvailableSlotDTO>());

        var slotDuration = (int)service.DurationInMinutes;
        
        // Retrieve schedules, checking if doctor is serving this service (Check 3)
        var scheduleBlocks = await _dbContext.Set<DoctorSchedule>()
            .Where(ds => ds.DoctorId == doctorId && ds.IsAvailable)
            .Include(ds => ds.Services)
            .Where(ds => ds.Services.Any(s => s.Id == serviceId))
            .ToListAsync();
        
        if (!scheduleBlocks.Any())
             return ((int)SlotRetrievalStatus.ServiceNotOffered, new List<AvailableSlotDTO>());


        // Retrieve all existing CONFIRMED and PENDING appointments
        var existingAppointments = await _dbContext.Set<Appointment>()
            .Where(a => a.DoctorId == doctorId && a.StartTime.Date >= today && 
                       (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending))
            .ToListAsync();

        // --- 2. Iterate and Generate Slots ---
        
        for (int i = 0; i < daysAhead; i++)
        {
            var targetDate = today.AddDays(i);
            var dayOfWeek = targetDate.DayOfWeek.ToString();
            var schedule = scheduleBlocks.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);

            if (schedule == null) continue;

            var slotStart = targetDate.Date.Add(schedule.StartTime);
            var scheduleEnd = targetDate.Date.Add(schedule.EndTime);

            while (slotStart.AddMinutes(slotDuration) <= scheduleEnd)
            {
                var slotEnd = slotStart.AddMinutes(slotDuration);

                var isSlotBooked = existingAppointments.Any(a => 
                    (a.StartTime >= slotStart && a.StartTime < slotEnd) || 
                    (a.StartTime < slotStart && a.StartTime.AddMinutes((double)a.BookedDurationMinutes) > slotStart) // do i overlap or im inside the some one appoointment
                );

                if (!isSlotBooked)
                {
                    availableSlots.Add(slotStart);
                }
                slotStart = slotStart.AddMinutes(slotDuration);
            }
        }
        
        // --- 3. Final Result ---
        if (!availableSlots.Any())
        {
            return ((int)SlotRetrievalStatus.NoSlotsAvailable, new List<AvailableSlotDTO>());
        }

        // Map final list to DTOs
        var slotDtos = availableSlots.Select(slotStart => new AvailableSlotDTO
        {
            SlotId = slotStart,
            DurationMinutes = slotDuration,
            DisplayTime = slotStart.ToString("ddd, MMM dd, h:mm tt") 
        }).ToList();

        return ((int)SlotRetrievalStatus.Success, slotDtos);
    }

    public async Task<(int, IReadOnlyList<FullDetailsDoctorDTO>)> SearchDoctorsAsync(DoctorSearchQueryDTO query)
    {

        Expression<Func<Doctor, bool>> criteria = d => true;
        if (query.DepartmentId.HasValue)
            criteria = criteria.And(d => d.DepartmentId == query.DepartmentId.Value);

        if (!string.IsNullOrEmpty(query.DoctorName))
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
        int count = await _unitOfWork.Repository<Doctor>().CountAsync(criteria);
        return (count, doctors);
    }
    public async Task<FullDetailsDoctorV1DTO> GetDoctorAsync(int id)
    {
        var doctor = await _unitOfWork.Context.Set<Doctor>()
        .Where(d => d.Id == id)
        .Include(d => d.Department)
        .Include(d => d.Clinics)
        .Include(d => d.Reviews)
        .ThenInclude(r => r.Patient).AsSplitQuery()
        .FirstOrDefaultAsync();

        var mappedDoctor = _mapper.Map<FullDetailsDoctorV1DTO>(doctor);
        return mappedDoctor;
    }

    public async Task<(int, IReadOnlyList<DepartmentWithDoctorsDTO>)> GetDepartmentsAsync(int? pageNumber = 1, int? pageSize = 1, int? DepartmentId = null)
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
            return (count, mappedDepartments);
    }
}
