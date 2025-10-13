using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingCore.Entities
{
    public class Patient : BaseEntity
    {

        public string? FullName { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; } 
        public DateTime? DateOfBirth { get; set; }
        [ForeignKey("User")]
        public string UserId { get; set; }
        public User User { get; set; }

        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}