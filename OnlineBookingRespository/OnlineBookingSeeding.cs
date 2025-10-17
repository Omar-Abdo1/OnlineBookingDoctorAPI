using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore.DTO.UserDTO;
using OnlineBookingCore.Entities;
using OnlineBookingRespository.Data;

namespace OnlineBookingRepository;

public static class OnlineBookingSeeding
{
    const string defaultPassword = "Pa$$w0rd";
    public static async Task SeedAsyncUsers(UserManager<User> userManager)
{
        const string defaultPassword = "Pa$$w0rd";
    if (!userManager.Users.Any())
    {
        // 1. Read the JSON file
          var usersData = File.ReadAllText("../OnlineBookingRespository/Data/DataSeeding/Users.json");
            var usersToSeed = JsonSerializer.Deserialize<List<UserSeedDTO>>(usersData);

            //C:\Users\Windows 10\Desktop\OnlineBookingDoctorAPI\OnlineBookingRespository\Data\DataSeeding\Users.json

            if (usersToSeed?.Count > 0)
            {
                foreach (var userSeed in usersToSeed)
                {
                    var user = new User()
                    {
                        Email = userSeed.email,
                        UserName = userSeed.username,
                        Role = userSeed.role,
                        PhoneNumber = userSeed.phoneNumber,
                        LastLogin = DateTime.MinValue
                    };
                    await userManager.CreateAsync(user, defaultPassword);
                }
            }
    }
}


    public static async Task SeedAsync(OnlineBookingContext context)
    {
        const string basePath = "../OnlineBookingRespository/Data/DataSeeding/";

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,// <-- Add this critical line for mismatched casing
            Converters = { new JsonStringEnumConverter() } // for enum string values
        };

        if (context.Departments.Any()) return; // DB has been seeded


        if (!context.Departments.Any())
        {
            // 1. Departments (No FKs)
            var departmentsData = await File.ReadAllTextAsync($"{basePath}Departments.json");
            var departments = JsonSerializer.Deserialize<List<Department>>(departmentsData, options);
            await context.Departments.AddRangeAsync(departments);
            await context.SaveChangesAsync();
        }

        if (!context.Clinics.Any())
        {
            // 2. Clinics (No FKs)
            var clinicsData = await File.ReadAllTextAsync($"{basePath}Clinic.json");
            var clinics = JsonSerializer.Deserialize<List<Clinic>>(clinicsData, options);
            await context.Clinics.AddRangeAsync(clinics);
            await context.SaveChangesAsync();
        }



        if (!context.Patients.Any())
        {
            // 3. Patients (FK to User, requires UserId GUIDs)
            var patientsData = await File.ReadAllTextAsync($"{basePath}Patients.json"); 
            var patients = JsonSerializer.Deserialize<List<Patient>>(patientsData, options);
            await context.Patients.AddRangeAsync(patients);
            await context.SaveChangesAsync();
        }

        if (!context.Doctors.Any())
        {
            // 4. Doctors (FK to User and Department)
            var doctorsData = await File.ReadAllTextAsync($"{basePath}Doctors.json");
            var doctors = JsonSerializer.Deserialize<List<Doctor>>(doctorsData, options);
            await context.Doctors.AddRangeAsync(doctors);
            await context.SaveChangesAsync();
        }


        if (!context.Services.Any())
        {
            // 5. Services (FK to Doctor)
            var servicesData = await File.ReadAllTextAsync($"{basePath}Services.json");
            var services = JsonSerializer.Deserialize<List<Service>>(servicesData, options);
            await context.Services.AddRangeAsync(services);
            await context.SaveChangesAsync();
        }

        if (!context.DoctorSchedules.Any())
        {
            // 6. Doctor Schedules (FK to Doctor)
            var schedulesData = await File.ReadAllTextAsync($"{basePath}DoctorSchdule.json");
            var schedules = JsonSerializer.Deserialize<List<DoctorSchedule>>(schedulesData, options);
            await context.DoctorSchedules.AddRangeAsync(schedules);
            await context.SaveChangesAsync();
        }


        if (!context.Appointments.Any())
        {
            // 9. Appointments (FK to Patient, Doctor, Schedule, Service)
            var appointmentsData = await File.ReadAllTextAsync($"{basePath}Appointments.json");
            //C:\Users\Windows 10\Desktop\OnlineBookingDoctorAPI\OnlineBookingRespository\Data\DataSeeding\Appointments.json
            var appointments = JsonSerializer.Deserialize<List<Appointment>>(appointmentsData, options);
            await context.Appointments.AddRangeAsync(appointments);
            await context.SaveChangesAsync();
        }

        if (!context.BillingRecords.Any())
        {
            var billingData = await File.ReadAllTextAsync($"{basePath}BillingRecord.json");
            //C:\Users\Windows 10\Desktop\OnlineBookingDoctorAPI\OnlineBookingRespository\Data\DataSeeding\BillingRecord.json
            var billingRecords = JsonSerializer.Deserialize<List<BillingRecord>>(billingData, options);
            await context.BillingRecords.AddRangeAsync(billingRecords);
            await context.SaveChangesAsync();
        }

        if (!context.Reviews.Any())
        {
            var reviewsData = await File.ReadAllTextAsync($"{basePath}Reviews.json");
            //C:\Users\Windows 10\Desktop\OnlineBookingDoctorAPI\OnlineBookingRespository\Data\DataSeeding\Reviews.json
            var reviews = JsonSerializer.Deserialize<List<Review>>(reviewsData, options);
            await context.Reviews.AddRangeAsync(reviews);
            await context.SaveChangesAsync();
        }

        // Many-to-Many Relationships Seeding

         var doctorClinicData = await File.ReadAllTextAsync($"{basePath}DoctorClinic.json");
        var doctorClinics = JsonSerializer.Deserialize<List<Dictionary<string, int>>>(doctorClinicData, options);
    // C:\Users\Windows 10\Desktop\OnlineBookingDoctorAPI\OnlineBookingRespository\Data\DataSeeding\DoctorClinic.json
        foreach (var dc in doctorClinics)
        {
            // SQL RAW INSERT: Inserting the many-to-many relationship explicitly
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO DoctorClinic (DoctorId, ClinicId) VALUES ({0}, {1})",
                dc["doctorId"], dc["clinicId"]
            );
        }

        var schServiceData = await File.ReadAllTextAsync($"{basePath}DoctorScheduleService.json");
        var scheduleServices = JsonSerializer.Deserialize<List<Dictionary<string, int>>>(schServiceData, options);
//C:\Users\Windows 10\Desktop\OnlineBookingDoctorAPI\OnlineBookingRespository\Data\DataSeeding\DoctorScheduleService.json
        foreach (var ss in scheduleServices)
        {
            // SQL RAW INSERT: Inserting the many-to-many relationship explicitly
            await context.Database.ExecuteSqlRawAsync(
                "INSERT INTO DoctorScheduleService (DoctorSchedulesId, ServicesId) VALUES ({0}, {1})",
                ss["doctorScheduleId"], ss["serviceId"]
            );
        }

    }

       

 
}
