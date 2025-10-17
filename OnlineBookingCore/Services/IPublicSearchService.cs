using System;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Schedule;

namespace OnlineBookingCore.Services;

public interface IPublicSearchService
{
    Task<FullDetailsDoctorV1DTO> GetDoctorAsync(int id);
    Task<(int, IReadOnlyList<DepartmentWithDoctorsDTO>)> GetDepartmentsAsync(int? pageNumber = 1, int? pageSize = 1, int? DepartmentId = null);
    Task<(int,IReadOnlyList<FullDetailsDoctorDTO>)> SearchDoctorsAsync(DoctorSearchQueryDTO query);
    Task<(int Status, IReadOnlyList<AvailableSlotDTO> Slots)> GetAvailableSlotsAsync(int doctorId, int serviceId, int daysAhead = 7);
}
