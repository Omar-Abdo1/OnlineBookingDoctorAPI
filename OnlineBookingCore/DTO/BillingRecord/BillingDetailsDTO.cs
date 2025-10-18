using System;

namespace OnlineBookingCore.DTO.BillingRecord;

public class BillingDetailsDTO
{
   public int RecordId { get; set; }
    public int AppointmentId { get; set; }
    public DateTime AppointmentTime { get; set; }
    public decimal Amount { get; set; }
    public bool IsPaid { get; set; }
    public string PatientName { get; set; }
}
