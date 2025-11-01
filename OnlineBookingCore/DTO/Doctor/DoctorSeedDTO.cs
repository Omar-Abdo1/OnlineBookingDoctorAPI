namespace OnlineBookingCore.DTO.Doctor;

public class DoctorSeedDTO
{
    public string userIdPlaceholder { get; set; } // Matches the new JSON field
    public int departmentId { get; set; }
    public string fullName { get; set; }
    public string address { get; set; }
    public int yearsOfExperience { get; set; }
    public bool isVerified { get; set; }
    public string phoneNumber { get; set; }
}