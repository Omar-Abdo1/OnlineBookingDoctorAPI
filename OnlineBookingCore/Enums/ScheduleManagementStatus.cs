namespace OnlineBookingCore.Enums;

public enum ScheduleManagementStatus
{
    Success,
    ScheduleNotFound,
    DoctorNotFound,
    NotOwner,
    DatabaseError,
    NotYourOwnService,
    ThereAreAppointmentBooked
}
