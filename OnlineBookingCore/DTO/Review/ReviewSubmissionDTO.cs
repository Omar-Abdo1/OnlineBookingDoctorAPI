using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookingCore.DTO.Review;

public class ReviewSubmissionDTO
{
     [Required]
    [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
    public int Rating { get; set; }
    [MaxLength(100)]
   public string? Comment { get; set; }
}
