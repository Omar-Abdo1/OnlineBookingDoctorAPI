using System;
using AutoMapper;
using OnlineBookingCore.DTO.Clinic;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Paitent;
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
        ////
        CreateMap<Doctor, FullDetailsDoctorDTO>();
        CreateMap<Doctor, FullDetailsDoctorV1DTO>();
        CreateMap<Department, DepartmentDTO>();
        CreateMap<Clinic, ClinicDTO>();
        CreateMap<Review, ReviewsDTO>().ForMember(dest => dest.ReviewerName,
         opt => opt.MapFrom(src => src.Patient.FullName));
        ////
        CreateMap<Patient,PaitentRegisterDTO>();
        CreateMap<PaitentRegisterDTO, Patient>().
        ForMember(dest => dest.FullName, opt => opt.Condition(src => src.FullName != null))
        .ForMember(dest => dest.Address, opt => opt.Condition(src => src.Address != null))
        .ForMember(dest => dest.PhoneNumber, opt => opt.Condition(src => src.PhoneNumber != null))
        .ForMember(dest => dest.DateOfBirth, opt => opt.Condition(src => src.DateOfBirth.HasValue));
        //////

    }
}
