using System;

namespace OnlineBookingCore.DTO.Doctor;

public class DoctorProfileUpdateDTO
{
    public string? FullName { get; set; }
    public string? Address { get; set; }
    public string? PhoneNumber { get; set; } 
    public int? YearsOfExperience { get; set; }
    public int? DepartmentId { get; set; }
}
