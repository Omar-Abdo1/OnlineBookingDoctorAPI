using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OnlineBookingAPI.Helpers;
using OnlineBookingCore.DTO.UserDTO;
using OnlineBookingCore.Entities;
using OnlineBookingCore.Services;
using OnlineBookingDoctorAPI.ErrorResponses;

namespace OnlineBookingAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Account : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;

        public Account(ITokenService tokenService, UserManager<User> userManager,SignInManager<User> signInManager)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("Register")]
        public async Task<ActionResult<UserToReturnDTO>> Register(RegisterDTO registerDTO)
        {

            if (await CheckEmailExist(registerDTO.Email))
                return BadRequest(new ApiErrorResponse(400, "Email is already in use"));


            var user = new User
            {
                Email = registerDTO.Email,
                UserName = registerDTO.UserName,
                Role = registerDTO.Role.ToUpper(),
                LastLogin = DateTime.UtcNow
            };

            var res = await _userManager.CreateAsync(user, registerDTO.Password);
            if (!res.Succeeded)
            {
                var errors = res.Errors.Select(e => e.Description).ToList();
                var errorMessage = $"Registration failed: {string.Join("; ", errors)}";
                return BadRequest(new ApiErrorResponse(400, errorMessage));
            }

            await _userManager.AddToRoleAsync(user, registerDTO.Role);

            return new UserToReturnDTO
            {
                Email = user.Email,
                Token = await _tokenService.CreateTokenAsync(user, _userManager)
            };
        }



        [HttpPost("Login")]
        public async Task<ActionResult<UserToReturnDTO>> Login(LoginDTO  login)
        {
            var user =await _userManager.FindByEmailAsync(login.Email);
            if (user is null) return BadRequest(new ApiErrorResponse(400, "Email is Not Correct"));
            var res = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
            if (!res.Succeeded) return BadRequest(new ApiErrorResponse(400, "Password is Not Correct"));

            user.LastLogin = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return new UserToReturnDTO()
            {
             Email = user.Email,
             Token = await _tokenService.CreateTokenAsync(user,_userManager)
            };
        }
        
        [HttpPost("Logout")]
        [Authorize] 
        public async Task<ActionResult> Logout()
{
    
    var user = await _userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
    
    if (user == null)
    {
        // The User record may have been deleted after the token was issued
        return NotFound(new ApiErrorResponse(404, "User not found."));
    }

    // REVOCATION ACTION: Update the Security Stamp
    // This method generates a new random GUID for the SecurityStamp field in the database.
    // Since this NEW stamp no longer matches the OLD stamp embedded in the client's JWT, 
    // the token is invalidated by the AuthorizeV1Filter middleware.
    await _userManager.UpdateSecurityStampAsync(user);

    return Content("You have been logged out successfully."); 
}

        [HttpGet("me")]
        [Authorize]
        [ServiceFilter(typeof(AuthorizeV1Filter))]
       public async Task<ActionResult<meDTO>> GetCurrentUser()
        {
            var user = await _userManager.FindByEmailAsync(User.FindFirstValue(ClaimTypes.Email));

            return new meDTO()
            {
                Email = user.Email,
                UserName = user.UserName,
                Role = user.Role
            };
        }
   
 
        [HttpGet("EmailExists")]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        public async Task<bool> CheckEmailExist([FromQuery]string Email)
        {
            var user = await _userManager.FindByEmailAsync(Email);
            return user is not null;
        }
        
    }
}
