using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Review;
using OnlineBookingCore.Enums;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
     [Authorize(Roles = "Patient")]
        [ServiceFilter(typeof(AuthorizeV1Filter))]
    public class ReviewController : ControllerBase
    {
        private readonly IUnitOfWork unitOfWork;
        private readonly IReviewService reviewService;

        public ReviewController(IUnitOfWork unitOfWork, IReviewService reviewService)
        {
            this.unitOfWork = unitOfWork;
            this.reviewService = reviewService;
        }

        [HttpPost("{doctorId:int}")] // Post api/review/doctorID
        public async Task<IActionResult> SubmitReview(int doctorID, ReviewSubmissionDTO reviewDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var status = await reviewService.SubmitReviewAsync(userId, doctorID, reviewDto);

            return status switch
            {
                ReviewStatus.Success => Created(string.Empty, new { Message = "Review submitted successfully." }),
                ReviewStatus.NotConfirmedOrCanceled => BadRequest(new ApiErrorResponse(404, "You can only review doctors after a confirmed appointment.")),
                ReviewStatus.AlreadyReviewed => Conflict(new ApiErrorResponse(409, "You have already reviewed this doctor.")),
                ReviewStatus.DoctorNotFound => NotFound(new ApiErrorResponse(404, "Doctor not found.")),
                ReviewStatus.ProfileNotFound => NotFound(new ApiErrorResponse(404, "Profile not found.")),
                ReviewStatus.MaxReviewsReached => BadRequest(new ApiErrorResponse(400, "You have reached the maximum number of reviews.")),
                _ => StatusCode(500, new ApiErrorResponse(500, "Failed to submit review due to a server error."))
            };
        }

        [HttpDelete("{id:int}")] // DELETE /api/review/{id}
        public async Task<IActionResult> DeleteReview(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var status = await reviewService.DeleteReviewAsync(id, userId);

            return status switch
            {
                ReviewStatus.Success => NoContent(), // 204 No Content for successful deletion
                ReviewStatus.ReviewNotFound => NotFound(new ApiErrorResponse(404, "Review not found or you do not own this review.")),
                ReviewStatus.NotYourReview => BadRequest(new ApiErrorResponse(400, "You do not own this review.")),
                _ => StatusCode(500, new ApiErrorResponse(500, "Failed to delete review."))
            };
        }

    }
}
