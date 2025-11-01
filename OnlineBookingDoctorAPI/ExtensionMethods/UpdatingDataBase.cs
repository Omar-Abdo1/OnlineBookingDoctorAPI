using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore.Entities;
using OnlineBookingRepository;
using OnlineBookingRespository.Data;

namespace OnlineBookingDoctorAPI.ExtensionMethods;

public static class UpdatingDataBase
{
   public static async Task UpdateDatabaseAsync(this IApplicationBuilder app)
   {
        using var scope = app.ApplicationServices.CreateScope();
        var Services = scope.ServiceProvider;
        var LoggerFactory = Services.GetRequiredService<ILoggerFactory>();
        try
        {           
                var dbContext = Services.GetRequiredService<OnlineBookingContext>();
               await dbContext.Database.MigrateAsync();

            // Seed data if necessary
            await OnlineBookingSeeding.SeedAsyncUsers(Services.GetRequiredService<UserManager<User>>() , Services.GetRequiredService<RoleManager<IdentityRole>>() );
            await OnlineBookingSeeding.SeedAsync(dbContext);
        }
        catch (Exception ex)
        {
            var Logger = LoggerFactory.CreateLogger<Program>();
            Logger.LogError(ex, "An error occurred while migrating the database.");
        }
   }
}
