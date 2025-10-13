using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingCore.Entities
{
    public class Doctor : BaseEntity
    {

        public string? FullName { get; set; }
        public string? Address { get; set; }
        public int? YearsOfExperience { get; set; }
        public bool IsVerified { get; set; }
        public string? PhoneNumber { get; set; } 

        [ForeignKey("User")]
        public int UserId { get; set; }
        public User User { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }
       public Department Department { get; set; }

        public ICollection<Service>? Services { get; set; }
        public ICollection<Clinic>? Clinics { get; set; }
        public ICollection<Appointment>? Appointments { get; set; }
        public ICollection<DoctorSchedule>? DoctorSchedules { get; set; }
        public ICollection<Review>? Reviews { get; set; }
    }
}