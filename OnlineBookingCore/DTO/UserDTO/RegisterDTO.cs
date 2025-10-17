using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineBookingCore.DTO.UserDTO;

public class RegisterDTO
{
    [EmailAddress , Required]
    public string Email { get; set; }
    [Required]
    public string Password { get; set; }
    [Required]
    public string UserName { get; set; }
    [Required]
    public string Role { get; set; }
}
