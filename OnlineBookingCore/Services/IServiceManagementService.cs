using System;
using OnlineBookingCore.DTO.Schedule;
using OnlineBookingCore.DTO.Service;
using OnlineBookingCore.Enums;

namespace OnlineBookingCore.Services;

public interface IServiceManagementService
{
    Task<IReadOnlyList<ServiceDetailsDTO>> GetDoctorServicesAsync(string doctorUserId);
    Task<(ServiceManagementStatus Status, int ServiceId)> CreateServiceAsync(string doctorUserId, ServiceCreationUpdateDTO dto);
    Task<ServiceManagementStatus> UpdateServiceAsync(int serviceId, string doctorUserId, ServiceCreationUpdateDTO dto);
    Task<ServiceManagementStatus> DeleteServiceAsync(int serviceId, string doctorUserId);
    //////////   
    Task<(ScheduleManagementStatus Status, int ScheduleId)> CreateScheduleAsync(string doctorUserId, ScheduleCreationDTO dto); 
    Task<ScheduleManagementStatus> UpdateScheduleAsync(int scheduleId, string doctorUserId, ScheduleCreationDTO dto);
    Task<ScheduleManagementStatus> DeleteScheduleAsync(int scheduleId, string doctorUserId);
    Task<List<ScheduleDetailsDTO>> GetSchedulesAsync(string doctorUserId);


}
