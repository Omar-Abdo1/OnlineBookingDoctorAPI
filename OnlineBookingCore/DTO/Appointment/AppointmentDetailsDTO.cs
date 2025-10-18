using System;

namespace OnlineBookingCore.DTO.Appointment;

public class AppointmentDetailsDTO
{
// --- Appointment Core Data ---
    public int AppointmentId { get; set; }
    public DateTime StartTime { get; set; }
    public decimal DurationMinutes { get; set; }
    public string Status { get; set; }

    // --- Service Details ---
    public string ServiceName { get; set; }
    public decimal ServicePrice { get; set; }
    
    // --- Billing Details ---
    public decimal AmountDue { get; set; }
    public bool IsPaid { get; set; }

    // --- Doctor/Provider Details ---
    public int DoctorId { get; set; }
    public string DoctorFullName { get; set; }
    public string DepartmentName { get; set; }

    // --- Patient Details ---
    public int PatientId { get; set; }
    public string PatientFullName { get; set; }
    public string PatientPhoneNumber { get; set; }
}
