
using OnlineBookingAPI.Helpers;
using OnlineBookingDoctorAPI.ExtensionMethods;
using OnlineBookingDoctorAPI.MiddleWares;
using StackExchange.Redis;

namespace OnlineBookingDoctorAPI;



public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers().AddJsonOptions(options =>
        {
            // Add the converter to use string names for all enums
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });
        builder.Services.AddControllers(options =>
        {
           // options.Filters.Add<MockDataFilter>();                  // enable mock filter
           // options.Conventions.Add(new MockDataRouteConvention()); // add mock routes
        });



        builder.Services.AddApplicationServices(builder.Configuration.GetConnectionString("DefaultConnection"));

        builder.Services.AddScoped<AuthorizeV1Filter>(); // Register the AuthorizeV1Filter

        builder.Services.AddJWTServices(builder.Configuration["JWT:SecretKey"],
         builder.Configuration["JWT:IssuerIP"]);


        builder.Services.AddSingleton<IConnectionMultiplexer>(options =>
        {
            var Connection = builder.Configuration.GetConnectionString("RedisConnection");
            return ConnectionMultiplexer.Connect(Connection);
        }); // For Redis and In Memory DataBase 


        builder.Services.AddSwaggerAdvanced("ONLINE BOOKING DOCTOR API");

        builder.Services.AddCors(options =>
       {
           options.AddPolicy("My Policy", options =>
           {
               options.AllowAnyHeader();
               options.AllowAnyMethod();
               options.WithOrigins(builder.Configuration["FrontBaseUrl"]); // URL for The FrontEnd
           });
       }); // for Angular Project 


        var app = builder.Build();

        await app.UpdateDatabaseAsync();


        app.UseMiddleware<ExceptionMiddleWare>(); // Register the custom exception middleware for global error handling
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseStaticFiles();

        app.UseStatusCodePagesWithReExecute("/error/{0}");

        app.UseHttpsRedirection();

        app.UseCors("My Policy");

        app.UseAuthentication();

        app.UseAuthorization();

        app.MapControllers();

        await app.RunAsync();
    }
}