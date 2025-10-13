using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingCore.Entities
{
    public class Service : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }
        public decimal DurationInMinutes { get; set; }
        
       [ForeignKey("Doctor")]
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
    }
}