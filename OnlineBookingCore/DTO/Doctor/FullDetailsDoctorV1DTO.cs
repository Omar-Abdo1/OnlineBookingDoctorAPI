using System;
using OnlineBookingCore.DTO.Clinic;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Review;

namespace OnlineBookingCore.DTO.Doctor;

public class FullDetailsDoctorV1DTO
{
       public string? FullName { get; set; }
        public string? Address { get; set; }
        public int? YearsOfExperience { get; set; }
        public bool IsVerified { get; set; }
        public string? PhoneNumber { get; set; } 
        public DepartmentDTO Department { get; set; }
        public ICollection<ClinicDTO> Clinics { get; set; }
        public ICollection<ReviewsDTO> reviews{ get; set; }
}
