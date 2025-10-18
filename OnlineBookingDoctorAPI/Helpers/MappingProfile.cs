using System;
using AutoMapper;
using OnlineBookingCore.DTO.Clinic;
using OnlineBookingCore.DTO.Department;
using OnlineBookingCore.DTO.Doctor;
using OnlineBookingCore.DTO.Paitent;
using OnlineBookingCore.DTO.Review;
using OnlineBookingCore.DTO.Service;
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
        
        CreateMap<Doctor, DoctorProfileDetailsDTO>()
           .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department.Name));

        CreateMap<DoctorProfileUpdateDTO, Doctor>()
        .ForMember(dest => dest.Id, opt => opt.Ignore())
        .ForMember(dest => dest.UserId, opt => opt.Ignore())
        .ForMember(dest => dest.IsVerified, opt => opt.Ignore()) // Doctor cannot verify themselves
        .ForMember(dest => dest.FullName, opt => opt.Condition(src => !string.IsNullOrEmpty(src.FullName)))
        .ForMember(dest => dest.Address, opt => opt.Condition(src => !string.IsNullOrEmpty(src.Address)))
        .ForMember(dest => dest.DepartmentId, opt => opt.Condition(src => src.DepartmentId.HasValue));

        ////////
        CreateMap<ServiceCreationUpdateDTO, Service>();
        CreateMap<Service, ServiceDetailsDTO>();

        /////////
         

    }
}
