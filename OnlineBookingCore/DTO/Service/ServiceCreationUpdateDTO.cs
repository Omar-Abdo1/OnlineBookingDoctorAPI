using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookingCore.DTO.Service;

public class ServiceCreationUpdateDTO
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [Required]
    [Range(1, 1000)]
    public decimal Price { get; set; } // Assuming price is between 1 and 1000
    
    [Required]
    [Range(1, 120)]
    public decimal DurationInMinutes { get; set; } // Assuming duration is 1-120 minutes
}
