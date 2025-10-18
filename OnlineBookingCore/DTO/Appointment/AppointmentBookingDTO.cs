using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookingCore.DTO.Appointment;

public class AppointmentBookingDTO
{
    [Required]
    public int DoctorId { get; set; }

    [Required]
    public int ServiceId { get; set; }

    [Required]
    public DateTime StartTime { get; set; }
}
