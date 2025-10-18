using System;
using OnlineBookingCore.DTO.Appointment;
using OnlineBookingCore.DTO.BillingRecord;

namespace OnlineBookingCore.Services;

public interface IAppointmentService
{
    Task<(int Status, int AppointmentId)> BookAppointmentAsync(string userId, AppointmentBookingDTO bookingDto);
    Task<(AppointmentDetailsDTO Details, bool IsOwner)> GetAppointmentDetailsAsync(string userId, int appointmentId);
    
    // For viewing history Patient
    Task<(int count , IReadOnlyList<AppointmentSummaryDTO>) > GetPatientAppointmentsAsync(string userId, int? pageIndex, int? pageSize);

    //  For patient cancellation
    Task<(bool Success, bool IsOwner)> CancelAppointmentAsync(int appointmentId, string userId);

    // For doctor confirmation
    Task<(bool Success, bool IsDoctor)> ConfirmAppointmentAsync(int appointmentId, string doctorUserId);


    //  For viewing history Doctor
    Task<IReadOnlyList<AppointmentSummaryDoctorDTO>> GetDoctorAppointmentsAsync(string doctorUserId);
    Task<IReadOnlyList<BillingDetailsDTO>> GetDoctorBillingHistoryAsync(string doctorUserId);
}

