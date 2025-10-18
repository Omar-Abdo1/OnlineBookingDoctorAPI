using System;

namespace OnlineBookingCore.DTO.Schedule;

public class ScheduleDetailsDTO : ScheduleCreationDTO
{
    public int ScheduleId { get; set; }
    public List<string> ServiceNames { get; set; } // For display
}