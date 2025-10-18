using System;

namespace OnlineBookingCore.DTO.Appointment;

public class AppointmentSummaryDoctorDTO
{
public int AppointmentId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string Status { get; set; }
    public string PatientName { get; set; }
    public string ServiceName { get; set; }
}
