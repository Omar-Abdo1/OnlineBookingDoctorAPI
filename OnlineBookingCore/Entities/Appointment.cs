using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingCore.Entities
{
    public class Appointment : BaseEntity
    {
        public DateTime StartTime { get; set; }
        public AppointmentStatus Status { get; set; }
        public decimal BookedDurationMinutes { get; set; }


        [ForeignKey("Patient")]
        public int PatientId { get; set; }
        public Patient Patient { get; set; }

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }

        [ForeignKey("DoctorSchedule")]
        public int DoctorScheduleId { get; set; }
        public DoctorSchedule DoctorSchedule { get; set; }

        public BillingRecord? BillingRecord { get; set; }

        [ForeignKey("Service")]
        public int ServiceId { get; set; }
        public Service Service { get; set; }
    }
}