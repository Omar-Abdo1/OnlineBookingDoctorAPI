using System;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Review;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Enums;
using OnlineBookingCore.Services;

namespace OnlineBookingService;

public class ReviewService : IReviewService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly DbContext _dbcontext;

    public ReviewService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
        _dbcontext = _unitOfWork.Context;
    }

    public async Task<ReviewStatus> SubmitReviewAsync(string patientUserId, int doctorId, ReviewSubmissionDTO reviewDto)
    {
        var patientProfile = await _unitOfWork.Repository<Patient>().GetEntityByConditionAsync(p => p.UserId == patientUserId);
        
        if (patientProfile == null) return ReviewStatus.ProfileNotFound;

        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d=>d.Id==doctorId);
        if (doctorProfile == null) return ReviewStatus.DoctorNotFound;

        // (MUST have had a Confirmed Appointment with this Doctor)
        var eligibleAppointmentCount = await _dbcontext.Set<Appointment>().CountAsync(a =>
            a.PatientId == patientProfile.Id &&
            a.DoctorId == doctorId &&
            a.Status == AppointmentStatus.Confirmed);

        if (eligibleAppointmentCount == 0) 
            return ReviewStatus.NotConfirmedOrCanceled;

        var CountReviews = await _dbcontext.Set<Review>().CountAsync(r => 
            r.PatientId == patientProfile.Id && r.DoctorId == doctorId);
            
        if (CountReviews>3) 
            return ReviewStatus.MaxReviewsReached ;

        var newReview = new Review
        {
            PatientId = patientProfile.Id,
            DoctorId = doctorId,
            Rating = reviewDto.Rating,
            Comment = reviewDto.Comment
        };
        
        await _unitOfWork.Repository<Review>().AddAsync(newReview);
        var result = await _unitOfWork.CompleteAsync();

        return result > 0 ? ReviewStatus.Success : ReviewStatus.DatabaseError;
    }

    public async Task<ReviewStatus> DeleteReviewAsync(int reviewId, string patientUserId)
    {
        var patientProfile = await _unitOfWork.Repository<Patient>().GetEntityByConditionAsync(p => p.UserId == patientUserId);
        if (patientProfile == null) return ReviewStatus.ReviewNotFound;

        var review = await _unitOfWork.Repository<Review>().GetEntityByConditionAsync(r => r.Id == reviewId);

        if (review == null) return ReviewStatus.ReviewNotFound;

        if (review.PatientId != patientProfile.Id)
            return ReviewStatus.NotYourReview;


        _unitOfWork.Repository<Review>().Delete(review);
        var result = await _unitOfWork.CompleteAsync();

        return result > 0 ? ReviewStatus.Success : ReviewStatus.DatabaseError;
    }
    
    public async Task<IReadOnlyList<ReviewDetailsDTO>> GetDoctorReviewsAsync(int doctorId)
{
    var doctor = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d=>d.Id==doctorId);
    if (doctor == null)
        return null; 
    

    var reviews = await _dbcontext.Set<Review>()
        .Where(r => r.DoctorId == doctorId)
        .Include(r => r.Patient)
        .Select(r => new ReviewDetailsDTO
        {
            ReviewId = r.Id,
            Rating = r.Rating,
            Comment = r.Comment,
            ReviewerName = r.Patient.FullName ?? "Deleted User" 
        })
        .OrderByDescending(r => r.ReviewId) // Order by latest review
        .ToListAsync();
    return reviews;
}

}
