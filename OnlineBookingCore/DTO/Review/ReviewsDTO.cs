using System;

namespace OnlineBookingCore.DTO.Review;

public class ReviewsDTO
{
         public int Rating { get; set; }
        public string? Comment { get; set; }
        public string ReviewerName { get; set; }
}
