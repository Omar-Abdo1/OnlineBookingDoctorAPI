using System;

namespace OnlineBookingCore.DTO.Schedule;

public class AvailableSlotDTO
{
// CRITICAL: The unique identifier sent back for booking. This is the exact StartTime.
    public DateTime SlotId { get; set; } 
    
    // Human-readable time representation
    public string DisplayTime { get; set; } 
    
    // Contextual data
    public decimal DurationMinutes { get; set; }
}
