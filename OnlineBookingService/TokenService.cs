using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Services;

namespace OnlineBookingService;

public class TokenService : ITokenService
{
      private readonly IConfiguration _config;

    public TokenService(IConfiguration  config)
    {
        _config = config;
    }

    public async Task<string> CreateTokenAsync(User user, UserManager<User> userManager)
    {

        
        // Fill the PayLoad

        var securityStamp = await userManager.GetSecurityStampAsync(user);

        var userClaims = new List<Claim>()
        {
            // predefined Claims : 
         new Claim(ClaimTypes.Email ,  user.Email),
         new Claim(ClaimTypes.MobilePhone, user.PhoneNumber),
         new Claim(ClaimTypes.Name,user.UserName),
         new Claim(ClaimTypes.NameIdentifier,user.Id),
         new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()) ,
         // Token Generated ID so it changes every Time

         // custom Claims : 
         new Claim("SecurityStamp",securityStamp) // for invoke the Last Login Time
        };
        var Roles = await userManager.GetRolesAsync(user);
        foreach (var role in Roles)
        userClaims.Add(new Claim(ClaimTypes.Role, role));

        
        var AuthKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_config["JWT:SecretKey"]));

        var Token = new JwtSecurityToken(
            issuer: _config["JWT:IssuerIP"],
            expires:DateTime.Now.AddDays(
                double.Parse(_config["JWT:DurationInDays"])) ,
            claims:userClaims,
            signingCredentials: new SigningCredentials(AuthKey , SecurityAlgorithms.HmacSha256Signature)
        );
        
        return new JwtSecurityTokenHandler().WriteToken(Token);
    }
}