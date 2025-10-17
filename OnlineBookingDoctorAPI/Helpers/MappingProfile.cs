using System;
using AutoMapper;
using OnlineBookingCore.DTO.Clinic;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Review;
using OnlineBookingCore.Entities;

namespace OnlineBookingAPI.Helpers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Create your mappings here
        CreateMap<Department, DepartmentWithDoctorsDTO>();
        CreateMap<Doctor, DoctorSummaryDTO>();

        CreateMap<Doctor, FullDetailsDoctorDTO>();
        CreateMap<Department, DepartmentDTO>();
        CreateMap<Clinic, ClinicDTO>();
        CreateMap<Review, ReviewsDTO>().ForMember(dest => dest.ReviewerName,
         opt => opt.MapFrom(src => src.Patient.FullName));
    }
}
