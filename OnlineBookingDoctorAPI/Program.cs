
using OnlineBookingDoctorAPI.MiddleWares;

namespace OnlineBookingDoctorAPI;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddAuthorization();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();
        app.UseMiddleware<ExceptionMiddleWare>(); // Register the custom exception middleware for global error handling
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStatusCodePagesWithReExecute("/error/{0}");  

        app.UseHttpsRedirection();

        app.UseAuthorization();
        
        await app.RunAsync();
    }
}