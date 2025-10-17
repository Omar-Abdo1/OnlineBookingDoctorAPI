using System;
using OnlineBookingCore.DTO.Clinic;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Review;
using OnlineBookingCore.Entities;

namespace OnlineBookingCore.DTO.Doctor;

public class FullDetailsDoctorDTO
{
       public string? FullName { get; set; }
        public string? Address { get; set; }
        public int? YearsOfExperience { get; set; }
        public bool IsVerified { get; set; }
        public string? PhoneNumber { get; set; } 
        public DepartmentDTO Department { get; set; }
        public double AverageRating { get; set; }
        public int ClinicsCount { get; set; }
        public int ReviewsCount { get; set; }
        public ICollection<string>? Addresses { get; set; }
}
