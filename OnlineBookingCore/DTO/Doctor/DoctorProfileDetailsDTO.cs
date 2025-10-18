using System;

namespace OnlineBookingCore.DTO.Doctor;

public class DoctorProfileDetailsDTO : DoctorProfileUpdateDTO
{
    public int DoctorId { get; set; }
    public string? DepartmentName { get; set; }
    public bool IsVerified { get; set; }
}
