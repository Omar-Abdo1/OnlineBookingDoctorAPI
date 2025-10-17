using System;

namespace OnlineBookingCore.DTO.Doctor;

public class DoctorSummaryDTO
{
       public int Id { get; set; }
       public string? FullName { get; set; }
        public string? Address { get; set; }
        public int? YearsOfExperience { get; set; }
        public bool IsVerified { get; set; }
        public string? PhoneNumber { get; set; } 
}
