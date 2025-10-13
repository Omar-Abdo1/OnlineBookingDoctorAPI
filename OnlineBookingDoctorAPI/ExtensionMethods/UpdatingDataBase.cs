using System;
using Microsoft.EntityFrameworkCore;
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

        }
        catch (Exception ex)
        {
            var Logger = LoggerFactory.CreateLogger<Program>();
            Logger.LogError(ex, "An error occurred while migrating the database.");
        }
   }
}
