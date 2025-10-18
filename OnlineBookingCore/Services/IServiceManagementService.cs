using System;
using OnlineBookingCore.DTO.Service;
using OnlineBookingCore.Enums;

namespace OnlineBookingCore.Services;

public interface IServiceManagementService
{
    Task<IReadOnlyList<ServiceDetailsDTO>> GetDoctorServicesAsync(string doctorUserId);
    Task<(ServiceManagementStatus Status, int ServiceId)> CreateServiceAsync(string doctorUserId, ServiceCreationUpdateDTO dto);
    Task<ServiceManagementStatus> UpdateServiceAsync(int serviceId, string doctorUserId, ServiceCreationUpdateDTO dto);
    Task<ServiceManagementStatus> DeleteServiceAsync(int serviceId, string doctorUserId);
}
