using System;
using AutoMapper;
using OnlineBookingCore;
using OnlineBookingCore.DTO.Service;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Enums;
using OnlineBookingCore.Services;

namespace OnlineBookingService;

public class ServiceManagementService : IServiceManagementService
{
   private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    
    public ServiceManagementService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    //Check Ownership and Existence
    private async Task<(Service Entity, bool IsOwner, ServiceManagementStatus Status)> GetServiceAndCheckOwner(int serviceId, int doctorProfileId)
    {
        var service = await _unitOfWork.Repository<Service>().GetEntityByConditionAsync(s=>s.Id==serviceId);
        if (service == null)
            return (null, false, ServiceManagementStatus.ServiceNotFound);
        
        if (service.DoctorId != doctorProfileId)
            return (service, false, ServiceManagementStatus.NotOwner);
            
        return (service, true, ServiceManagementStatus.Success);
    }

    // CREATE Service
    public async Task<(ServiceManagementStatus Status, int ServiceId)> CreateServiceAsync(string doctorUserId, ServiceCreationUpdateDTO dto)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null)
            return (ServiceManagementStatus.DoctorNotFound, 0);

        var newService = _mapper.Map<Service>(dto);
        newService.DoctorId = doctorProfile.Id; 
        
        await _unitOfWork.Repository<Service>().AddAsync(newService);
        var result = await _unitOfWork.CompleteAsync();

        return result > 0 
            ? (ServiceManagementStatus.Success, newService.Id) 
            : (ServiceManagementStatus.DatabaseError, 0);
    }

    // RETRIEVE Services 
    public async Task<IReadOnlyList<ServiceDetailsDTO>> GetDoctorServicesAsync(string doctorUserId)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null)
            return new List<ServiceDetailsDTO>(); 
        
        var services = await _unitOfWork.Repository<Service>()
            .GetAllByConditionAsync(criteria: s => s.DoctorId == doctorProfile.Id);
            
        return _mapper.Map<IReadOnlyList<ServiceDetailsDTO>>(services);
    }

    // UPDATE Service 
    public async Task<ServiceManagementStatus> UpdateServiceAsync(int serviceId, string doctorUserId, ServiceCreationUpdateDTO dto)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return ServiceManagementStatus.DoctorNotFound;
        
        var (existingService, isOwner, status) = await GetServiceAndCheckOwner(serviceId, doctorProfile.Id);
        
        if (status != ServiceManagementStatus.Success) return status;

        _mapper.Map(dto, existingService);
        
        _unitOfWork.Repository<Service>().Update(existingService);
        var result = await _unitOfWork.CompleteAsync();

        return result > 0 ? ServiceManagementStatus.Success : ServiceManagementStatus.DatabaseError;
    }

    // DELETE Service
    public async Task<ServiceManagementStatus> DeleteServiceAsync(int serviceId, string doctorUserId)
    {
        var doctorProfile = await _unitOfWork.Repository<Doctor>().GetEntityByConditionAsync(d => d.UserId == doctorUserId);
        if (doctorProfile == null) return ServiceManagementStatus.DoctorNotFound;

        var (existingService, isOwner, status) = await GetServiceAndCheckOwner(serviceId, doctorProfile.Id);
        
        if (status != ServiceManagementStatus.Success) return status;
        
        _unitOfWork.Repository<Service>().Delete(existingService);
        var result = await _unitOfWork.CompleteAsync();
        return result > 0 ? ServiceManagementStatus.Success : ServiceManagementStatus.DatabaseError;
    }
}
