using System;

namespace OnlineBookingCore.DTO.Review;

public class ReviewDetailsDTO
{
public int ReviewId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string ReviewerName { get; set; } 
}
