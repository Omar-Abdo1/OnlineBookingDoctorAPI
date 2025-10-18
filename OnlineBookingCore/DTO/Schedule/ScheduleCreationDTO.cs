using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookingCore.DTO.Schedule;

public class ScheduleCreationDTO
{
   [Required]
    public string DayOfWeek { get; set; } 
    
    [Required]
    public TimeSpan StartTime { get; set; } 
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public bool IsAvailable { get; set; } = true;
    [Required]
    public List<int>? ServiceIds { get; set; }
}
