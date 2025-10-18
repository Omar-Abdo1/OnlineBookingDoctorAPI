using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Repositories;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;
using OnlineBookingRespository;
using OnlineBookingRespository.Data;
using OnlineBookingService;

namespace OnlineBookingDoctorAPI.ExtensionMethods
{
    public static class ApplicationServicesExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<OnlineBookingContext>(options =>
            {
                options.UseSqlServer(connectionString);
            });

            services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<OnlineBookingContext>();

            // Repositotries :
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();



            // Services :
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IResponseCacheService, ResponseCacheService>();
            services.AddScoped<IPublicSearchService, PublicSearchService>();
            services.AddScoped<IAppointmentService,AppointmentService>();
            services.AddAutoMapper(typeof(MappingProfile));



             #region Validation Error 
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = (context) =>
            {
                var errors = context.ModelState.Where(P => P.Value.Errors.Count() > 0)
                    .SelectMany(P => P.Value.Errors)
                    .Select(E => E.ErrorMessage)
                    .ToList();
                var ValidationErrorReposonse = new ApiValidationResponse()
                {
                    Errors = errors
                };
                return new BadRequestObjectResult(ValidationErrorReposonse);

            };
        }); // For Validation Error 
        #endregion

            return services;
        }
    }
}