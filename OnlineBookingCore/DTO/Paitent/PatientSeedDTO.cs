namespace OnlineBookingCore.DTO.Paitent;

public class PatientSeedDTO
{
    public string userIdPlaceholder { get; set; } // Matches the new JSON field
    public string fullName { get; set; }
    public string address { get; set; }
    public string phoneNumber { get; set; }
    public DateTime dateOfBirth { get; set; }
}