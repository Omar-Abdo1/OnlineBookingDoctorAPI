namespace OnlineBookingCore.Enums;

public enum ReviewStatus
{
    Success,
    AppointmentNotFound,
    NotConfirmedOrCanceled, // Appointment status must be complete to review
    DoctorNotFound,
    AlreadyReviewed,
    DatabaseError,
    ReviewNotFound,
    ProfileNotFound,
    MaxReviewsReached,
   NotYourReview,
}
