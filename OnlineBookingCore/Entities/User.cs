using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace OnlineBookingCore.Entities
{
    public class User : IdentityUser
    {
        public string Role{ get; set; }
        public DateTime? LastLogin { get; set; }
    }
}