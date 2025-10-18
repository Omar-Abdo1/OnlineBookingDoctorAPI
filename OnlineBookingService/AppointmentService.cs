using System;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Appointment;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Services;

namespace OnlineBookingService;

public class AppointmentService : IAppointmentService
{
    private readonly IUnitOfWork unitOfWork;
    private readonly IMapper mapper;

    public enum BookingStatus
{
    Success,
    DoctorOrServiceNotFound,
    SlotAlreadyBooked, // The race condition failure
    ScheduleMismatch, // The chosen slot is not valid for the doctor/service
    ProfileNotFound, // Patient profile hasn't been created yet
    DatabaseError
}
       

      public AppointmentService(IUnitOfWork unitOfWork,IMapper mapper)
      {
        this.unitOfWork = unitOfWork;
        this.mapper = mapper;
    }

    public async Task<(int Status, int AppointmentId)> BookAppointmentAsync(string userId, AppointmentBookingDTO bookingDto)
    {
        var dbcontext = unitOfWork.Context;
        using var transaction = dbcontext.Database.BeginTransaction();
        try
        {
            var Patient = await unitOfWork.Repository<Patient>().
            GetEntityByConditionAsync(p => p.UserId == userId);
            if (Patient == null)
                return ((int)BookingStatus.ProfileNotFound, 0);

            var service = await unitOfWork.Repository<Service>().
            GetEntityByConditionAsync(s => s.Id == bookingDto.ServiceId);

            var doctor = await unitOfWork.Repository<Doctor>().
            GetEntityByConditionAsync(d => d.Id == bookingDto.DoctorId);

            if (service == null || doctor == null)
                return ((int)BookingStatus.DoctorOrServiceNotFound, 0);

            var slotEnd = bookingDto.StartTime.AddMinutes((double)service.DurationInMinutes);

            var isSlotTaken = await dbcontext.Set<Appointment>().AnyAsync(a =>
                a.DoctorId == bookingDto.DoctorId &&
                (a.Status == AppointmentStatus.Confirmed || a.Status == AppointmentStatus.Pending) &&
                // Check if existing appointment starts within the new slot OR overlaps it
                ((a.StartTime >= bookingDto.StartTime && a.StartTime < slotEnd) ||
                 (a.StartTime < bookingDto.StartTime && a.StartTime.AddMinutes((double)a.BookedDurationMinutes) > bookingDto.StartTime))
            );

            if (isSlotTaken)
                return ((int)BookingStatus.SlotAlreadyBooked, 0);

            var dayOfWeek = bookingDto.StartTime.DayOfWeek.ToString();
            var scheduleBlock = await dbcontext.Set<DoctorSchedule>()
                 .Where(ds => ds.DoctorId == bookingDto.DoctorId && ds.DayOfWeek == dayOfWeek && ds.IsAvailable)
                 .Include(ds => ds.Services)
                 .Where(ds => ds.Services.Any(s => s.Id == bookingDto.ServiceId))
                 .FirstOrDefaultAsync();

            if (scheduleBlock == null) return ((int)BookingStatus.ScheduleMismatch, 0);

            var appointment = new Appointment
            {
                PatientId = Patient.Id,
                DoctorId = bookingDto.DoctorId,
                ServiceId = bookingDto.ServiceId,
                StartTime = bookingDto.StartTime,
                BookedDurationMinutes = service.DurationInMinutes,
                Status = AppointmentStatus.Pending,
                DoctorScheduleId = scheduleBlock.Id
            };

            await unitOfWork.Repository<Appointment>().AddAsync(appointment);
            await unitOfWork.CompleteAsync(); // need to save it to got the ID from DB because it is identity 

            var billingRecord = new BillingRecord
            {
                AppointmentId = appointment.Id,
                Amount = service.Price,
                IsPaid = false
            };

            await unitOfWork.Repository<BillingRecord>().AddAsync(billingRecord);
            await unitOfWork.CompleteAsync();

            await transaction.CommitAsync();
            return ((int)BookingStatus.Success, appointment.Id);

        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return ((int)BookingStatus.DatabaseError, 0);
        }



    }

    public async Task<(AppointmentDetailsDTO Details, bool IsOwner)> GetAppointmentDetailsAsync(string userId, int appointmentId)
    {
        var dbcontext = unitOfWork.Context;

        var appointment = await dbcontext.Set<Appointment>().AsNoTracking()
        .Include(a => a.Patient)
        .Include(a => a.Doctor).
        ThenInclude(d => d.Department)
        .Include(a => a.Service)
        .Include(a => a.BillingRecord).AsSplitQuery()
        .FirstOrDefaultAsync(a => a.Id == appointmentId);

    if (appointment == null)
        return (null, false);
    
    
    var isPatient = appointment.Patient.UserId == userId;
    var isDoctor = appointment.Doctor.UserId == userId;
    
    if (isPatient==false && isDoctor==false)
    {
        return (null, false); // Not the owner, reject access
    }

    var details = new AppointmentDetailsDTO
    {
        AppointmentId = appointment.Id,
        StartTime = appointment.StartTime,
        DurationMinutes = appointment.BookedDurationMinutes,
        Status = appointment.Status.ToString(),

        ServicePrice = appointment.Service.Price,
        ServiceName = appointment.Service.Name,
        
        AmountDue = appointment.BillingRecord?.Amount ?? 0,
        IsPaid = appointment.BillingRecord?.IsPaid ?? false,

        DoctorId = appointment.DoctorId,
        DoctorFullName = appointment.Doctor.FullName,
        DepartmentName = appointment.Doctor.Department?.Name,

        PatientId = appointment.PatientId,
        PatientFullName = appointment.Patient.FullName,
        PatientPhoneNumber =  appointment.Patient.PhoneNumber
    };

    return (details, true);

    }

   public async Task<(int count,IReadOnlyList<AppointmentSummaryDTO>)> GetPatientAppointmentsAsync(string userId, int? pageIndex, int? pageSize)
    {


        int count = 0;
    
    var patientProfile = await unitOfWork.Repository<Patient>()
        .GetEntityByConditionAsync(p => p.UserId == userId);

        if (patientProfile == null)
            return (count, new List<AppointmentSummaryDTO>());

        var dbcontext = unitOfWork.Context;

    var query = dbcontext.Set<Appointment>()
        .Where(a => a.PatientId == patientProfile.Id)
        .OrderByDescending(a => a.StartTime)
        .AsNoTracking();

    count = await query.CountAsync();

    var appointments = await query
        .Skip((pageIndex.Value - 1) * pageSize.Value)
        .Take(pageSize.Value)
        .Include(a => a.Doctor)
        .Include(a => a.Service)
        .Select(a => new AppointmentSummaryDTO 
        {
            AppointmentId = a.Id,
            AppointmentDate = a.StartTime,
            Status = a.Status.ToString(),
            DoctorName = a.Doctor.FullName,
            ServiceName = a.Service.Name
        })
        .ToListAsync();

    return (count,appointments);
}

   public async Task<(bool Success, bool IsOwner)> CancelAppointmentAsync(int appointmentId, string userId)
{
    var patientProfile = await unitOfWork.Repository<Patient>()
        .GetEntityByConditionAsync(p => p.UserId == userId);
    
    if (patientProfile == null) return (false, false);

        var appointment = await unitOfWork.Repository<Appointment>().GetEntityByConditionAsync(a => a.Id == appointmentId);
    
    if (appointment == null) return (false, true); // Appointment not found, but user is technically owner-capable

    // Owner Check
    if (appointment.PatientId != patientProfile.Id)
        return (false, false); // Not the owner
    
    if (appointment.Status == AppointmentStatus.Canceled)
    {
        return (true, true); // Already canceled, return success
    }
    
    // Update status
    appointment.Status = AppointmentStatus.Canceled;
    unitOfWork.Repository<Appointment>().Update(appointment);

    var result = await unitOfWork.CompleteAsync();

    return (result > 0, true);
}

    public async Task<(bool Success, bool IsDoctor)> ConfirmAppointmentAsync(int appointmentId, string doctorUserId)
{
    var doctorProfile = await unitOfWork.Repository<Doctor>()
        .GetEntityByConditionAsync(d => d.UserId == doctorUserId);
    
    if (doctorProfile == null) return (false, false); // Not a recognized Doctor user

    var appointment = await unitOfWork.Repository<Appointment>().GetEntityByConditionAsync(a=>a.Id==appointmentId);
    
    if (appointment == null) return (false, true);

    // Doctor Owner Check
    if (appointment.DoctorId != doctorProfile.Id)
        return (false, false); 
    

    if (appointment.Status != AppointmentStatus.Pending)
    {
        return (true, true); // Already confirmed/canceled, return success
    }
    
    // Update status
    appointment.Status = AppointmentStatus.Confirmed;
    unitOfWork.Repository<Appointment>().Update(appointment);

    var result = await unitOfWork.CompleteAsync();

    return (result > 0, true);
}
}
