using System;

namespace OnlineBookingCore.DTO.UserDTO;

public class UserSeedDTO
{
    public string idPlaceholder { get; set; } // We use this for mapping after creation
public string username { get; set; }
    public string email { get; set; }
    public string role { get; set; }
    public string phoneNumber { get; set; }
}
