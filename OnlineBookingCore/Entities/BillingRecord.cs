using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingCore.Entities
{
    public class BillingRecord : BaseEntity
    {
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }

        [ForeignKey("Appointment")]
        
        public int AppointmentId { get; set; }
        public Appointment Appointment { get; set; }
    }
}