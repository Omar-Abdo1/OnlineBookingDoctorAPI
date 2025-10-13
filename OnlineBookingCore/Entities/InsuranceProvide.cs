using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineBookingCore.Entities
{
    public class InsuranceProvider : BaseEntity
    {
        public string Name { get; set; }
        public string ContactNumber { get; set; }
    }
}