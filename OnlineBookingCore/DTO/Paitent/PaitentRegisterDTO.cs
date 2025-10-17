using System;

namespace OnlineBookingCore.DTO.Paitent;

public class PaitentRegisterDTO
{
       public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; } 
        public DateTime? DateOfBirth { get; set; }
}
