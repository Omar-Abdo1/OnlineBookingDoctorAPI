using System.Runtime.Serialization;

namespace OnlineBookingCore.Entities
{
    public enum AppointmentStatus
    {
        [EnumMember(Value = "Pending")]
        Pending = 1, 
        [EnumMember(Value = "Confirmed")]
        Confirmed = 4,
        [EnumMember(Value = "Canceled")]
        Canceled = 8
    }
}