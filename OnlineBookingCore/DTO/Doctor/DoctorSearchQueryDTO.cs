using System;

namespace OnlineBookingCore.DTO.Doctor;

public class DoctorSearchQueryDTO
{
  public int? pageSize { get; set; } = 5;
  public int? pageNumber { get; set; } = 1;
  public int? DepartmentId { get; set; }
  public string? DoctorName { get; set; }
}
