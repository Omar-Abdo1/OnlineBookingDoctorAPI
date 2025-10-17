using System;
using Microsoft.AspNetCore.Identity;
using OnlineBookingCore.Entities;

namespace OnlineBookingCore.Services;

public interface ITokenService
{
    Task<string> CreateTokenAsync(User user, UserManager<User> userManager);
}
