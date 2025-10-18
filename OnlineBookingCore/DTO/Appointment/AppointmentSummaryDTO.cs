using System;

namespace OnlineBookingCore.DTO.Appointment;

public class AppointmentSummaryDTO
{
    public int AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; }
    public string DoctorName { get; set; }
    public string ServiceName { get; set; }
}
