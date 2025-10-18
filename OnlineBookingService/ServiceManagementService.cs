using System;
using System.Data.Common;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Schedule;
using OnlineBookingCore.DTO.Service;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Enums;
using OnlineBookingCore.Services;

namespace OnlineBookingService;

public class ServiceManagementService : IServiceManagementService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly DbContext _context;

    public ServiceManagementService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = _unitOfWork.Context;
    }

    //Check Ownership and Existence
    private async Task<(Service Entity, bool IsOwner, ServiceManagementStatus Status)> GetServiceAndCheckOwner(int serviceId, int doctorProfileId)
    {
        var service = await _unitOfWork.Repository<Service>().GetEntityByConditionAsync(s => s.Id == serviceId);
        if (service == null)
            return (null, false, ServiceManagementStatus.ServiceNotFound);

        if (service.DoctorId != doctorProfileId)
            return (service, false, ServiceManagementStatus.NotOwner);

        return (service, true, ServiceManagementStatus.Success);
    }

    // CREATE Service
    public async Task<(ServiceManagementStatus Status, int ServiceId)> CreateServiceAsync(string doctorUserId, ServiceCreationUpdateDTO dto)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null)
            return (ServiceManagementStatus.DoctorNotFound, 0);

        var newService = _mapper.Map<Service>(dto);
        newService.DoctorId = doctorProfile.Id;

        await _unitOfWork.Repository<Service>().AddAsync(newService);
        var result = await _unitOfWork.CompleteAsync();

        return result > 0
            ? (ServiceManagementStatus.Success, newService.Id)
            : (ServiceManagementStatus.DatabaseError, 0);
    }

    // RETRIEVE Services 
    public async Task<IReadOnlyList<ServiceDetailsDTO>> GetDoctorServicesAsync(string doctorUserId)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null)
            return new List<ServiceDetailsDTO>();

        var services = await _unitOfWork.Repository<Service>()
            .GetAllByConditionAsync(criteria: s => s.DoctorId == doctorProfile.Id);

        return _mapper.Map<IReadOnlyList<ServiceDetailsDTO>>(services);
    }

    // UPDATE Service 
    public async Task<ServiceManagementStatus> UpdateServiceAsync(int serviceId, string doctorUserId, ServiceCreationUpdateDTO dto)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return ServiceManagementStatus.DoctorNotFound;

        var (existingService, isOwner, status) = await GetServiceAndCheckOwner(serviceId, doctorProfile.Id);

        if (status != ServiceManagementStatus.Success) return status;

        _mapper.Map(dto, existingService);

        _unitOfWork.Repository<Service>().Update(existingService);
        var result = await _unitOfWork.CompleteAsync();

        return result > 0 ? ServiceManagementStatus.Success : ServiceManagementStatus.DatabaseError;
    }

    // DELETE Service
    public async Task<ServiceManagementStatus> DeleteServiceAsync(int serviceId, string doctorUserId)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return ServiceManagementStatus.DoctorNotFound;

        var (existingService, isOwner, status) = await GetServiceAndCheckOwner(serviceId, doctorProfile.Id);

        if (status != ServiceManagementStatus.Success) return status;

        _unitOfWork.Repository<Service>().Delete(existingService);
        var result = await _unitOfWork.CompleteAsync();
        return result > 0 ? ServiceManagementStatus.Success : ServiceManagementStatus.DatabaseError;
    }


    // Create Schedule
    public async Task<(ScheduleManagementStatus Status, int ScheduleId)> CreateScheduleAsync(string doctorUserId, ScheduleCreationDTO dto)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return (ScheduleManagementStatus.DoctorNotFound, 0);

        var newSchedule = _mapper.Map<DoctorSchedule>(dto);
        newSchedule.DoctorId = doctorProfile.Id;

        var doctorServiceIds = await _context.Set<Service>()
            .Where(s => s.DoctorId == doctorProfile.Id)
            .Select(s => s.Id).ToListAsync();

        if (dto.ServiceIds.Any(id => !doctorServiceIds.Contains(id)))
        {
            return (ScheduleManagementStatus.NotYourOwnService, 0);
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await _unitOfWork.Repository<DoctorSchedule>().AddAsync(newSchedule);
            await _unitOfWork.CompleteAsync(); // Save to get the newSchedule.Id PK

            foreach (var serviceId in dto.ServiceIds)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO DoctorScheduleService (DoctorSchedulesId, ServicesId) VALUES ({0}, {1})",
                    newSchedule.Id, serviceId
                );
            }
            //[dbo].[DoctorScheduleService]

            await transaction.CommitAsync();
            return (ScheduleManagementStatus.Success, newSchedule.Id);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return (ScheduleManagementStatus.DatabaseError, 0);
        }
    }

    // Get Schedules
    public async Task<List<ScheduleDetailsDTO>> GetSchedulesAsync(string doctorUserId)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return new List<ScheduleDetailsDTO>();

        var schedules = await _context.Set<DoctorSchedule>()
            .Where(ds => ds.DoctorId == doctorProfile.Id)
            .Include(ds => ds.Services)
            .OrderBy(ds => ds.DayOfWeek).ThenBy(ds => ds.StartTime)
            .ToListAsync();

        return schedules.Select(ds => new ScheduleDetailsDTO
        {
            ScheduleId = ds.Id,
            DayOfWeek = ds.DayOfWeek,
            StartTime = ds.StartTime,
            EndTime = ds.EndTime,
            IsAvailable = ds.IsAvailable,
            ServiceNames = ds.Services.Select(s => s.Name).ToList()
        })
        .ToList();
    }

    // Delete Schedule
    public async Task<ScheduleManagementStatus> DeleteScheduleAsync(int scheduleId, string doctorUserId)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return ScheduleManagementStatus.DoctorNotFound;

        var schedule = await _unitOfWork.Repository<DoctorSchedule>().GetEntityByConditionAsync(s => s.Id == scheduleId);
        if (schedule == null) return ScheduleManagementStatus.ScheduleNotFound;

        if (schedule.DoctorId != doctorProfile.Id) return ScheduleManagementStatus.NotOwner;

        //Ensure no future appointments are booked in this schedule
        var futureAppointmentsExist = await _context.Set<Appointment>()
            .AnyAsync(a => a.DoctorScheduleId == scheduleId && a.StartTime > DateTime.UtcNow);

        if (futureAppointmentsExist)
        {
            return ScheduleManagementStatus.ThereAreAppointmentBooked;
        }

        _unitOfWork.Repository<DoctorSchedule>().Delete(schedule);
        var result = await _unitOfWork.CompleteAsync();
        return result > 0 ? ScheduleManagementStatus.Success : ScheduleManagementStatus.DatabaseError;
    }


    // Update Schedule
    public async Task<ScheduleManagementStatus> UpdateScheduleAsync(int scheduleId, string doctorUserId, ScheduleCreationDTO dto)
    {
        // --- 1. INITIAL VALIDATION & OWNERSHIP CHECK ---
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return ScheduleManagementStatus.DoctorNotFound;

        var existingSchedule = await _context.Set<DoctorSchedule>()
            .Include(ds => ds.Services)
            .FirstOrDefaultAsync(ds => ds.Id == scheduleId);

        if (existingSchedule == null) return ScheduleManagementStatus.ScheduleNotFound;

        if (existingSchedule.DoctorId != doctorProfile.Id) return ScheduleManagementStatus.NotOwner;

        var futureAppointmentsExist = await _context.Set<Appointment>()
            .AnyAsync(a => a.DoctorScheduleId == scheduleId && a.StartTime > DateTime.UtcNow);

        if (futureAppointmentsExist)
            return ScheduleManagementStatus.ThereAreAppointmentBooked;

        if (dto.ServiceIds != null && dto.ServiceIds.Any())
        {
            var doctorServiceIds = await _context.Set<Service>()
                .Where(s => s.DoctorId == doctorProfile.Id)
                .Select(s => s.Id).ToListAsync();

            if (dto.ServiceIds.Any(id => !doctorServiceIds.Contains(id)))
            {
                return ScheduleManagementStatus.NotYourOwnService;
            }
        }


        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {

            _mapper.Map(dto, existingSchedule);

            if (dto.ServiceIds != null)
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "DELETE FROM DoctorScheduleService WHERE DoctorSchedulesId = {0}", scheduleId);

                foreach (var serviceId in dto.ServiceIds)
                {
                    await _context.Database.ExecuteSqlRawAsync(
                        "INSERT INTO DoctorScheduleService (DoctorSchedulesId, ServicesId) VALUES ({0}, {1})",
                        scheduleId, serviceId
                    );
                }
            }
            _unitOfWork.Repository<DoctorSchedule>().Update(existingSchedule);
            var result = await _unitOfWork.CompleteAsync();

            if (result > 0)
            {
                await transaction.CommitAsync();
                return ScheduleManagementStatus.Success;
            }
            else
            {
                await transaction.RollbackAsync();
                return ScheduleManagementStatus.DatabaseError;
            }

        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return ScheduleManagementStatus.DatabaseError;
        }
    }
  
  // AssociateClinic
   public async Task<ServiceManagementStatus> AssociateClinicAsync(string doctorUserId, int clinicId)
{
    var doctorProfile = await _unitOfWork.Repository<Doctor>()
        .GetEntityByConditionAsync(d => d.UserId == doctorUserId);
    
    if (doctorProfile == null) return ServiceManagementStatus.DoctorNotFound;

    var clinicExists = await _unitOfWork.Repository<Clinic>().GetEntityByConditionAsync(c=>c.Id==clinicId);
    if (clinicExists == null) return ServiceManagementStatus.ServiceNotFound;

        var linkExists = await _unitOfWork.Context.Set<Clinic>()
        .Include(c => c.Doctors)
        .AnyAsync(c => c.Id == clinicId && c.Doctors.Any(d => d.Id == doctorProfile.Id));
    
    if (linkExists) return ServiceManagementStatus.Success; 

    using var transaction = await _unitOfWork.Context.Database.BeginTransactionAsync();
    try
    {
            await _unitOfWork.Context.Database.ExecuteSqlRawAsync(
                "INSERT INTO DoctorClinic (DoctorId, ClinicId) VALUES ({0}, {1})",
                doctorProfile.Id, clinicId
            );
        //[dbo].[DoctorClinic]
        await transaction.CommitAsync();
        return ServiceManagementStatus.Success;
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        return ServiceManagementStatus.DatabaseError;
    }
} 
}
