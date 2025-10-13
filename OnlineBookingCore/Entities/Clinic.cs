using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingCore.Entities
{
    public class Clinic : BaseEntity
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public ICollection<Doctor>? Doctors { get; set; }
    }
}