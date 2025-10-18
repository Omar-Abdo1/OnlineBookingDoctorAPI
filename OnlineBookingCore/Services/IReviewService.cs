using System;
using OnlineBookingCore.DTO.Review;
using OnlineBookingCore.Enums;

namespace OnlineBookingCore.Services;

public interface IReviewService
{
// Submitting a new review
    Task<ReviewStatus> SubmitReviewAsync(string patientUserId, int doctorId, ReviewSubmissionDTO reviewDto);
    
    // Deleting an existing review
    Task<ReviewStatus> DeleteReviewAsync(int reviewId, string patientUserId);
    Task<IReadOnlyList<ReviewDetailsDTO>> GetDoctorReviewsAsync(int doctorId);
}
