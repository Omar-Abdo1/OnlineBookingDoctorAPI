using System;
using OnlineBookingCore.DTO.Doctor;

namespace OnlineBookingCore.DTO.Department;

public class DepartmentWithDoctorsDTO
{
     public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public IReadOnlyList<DoctorSummaryDTO> Doctors { get; set; }
}
