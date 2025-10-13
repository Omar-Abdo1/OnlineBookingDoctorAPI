using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineBookingCore.Entities;

namespace OnlineBookingRespository.Data;

public class OnlineBookingContext : IdentityDbContext<User>
{
    public OnlineBookingContext(DbContextOptions<OnlineBookingContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        builder.Entity<User>().ToTable("Users");
        builder.Entity<IdentityRole>().ToTable("Roles");
        builder.Entity<IdentityUserRole<string>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<string>>().ToTable("UserTokens");
       

       // many to many between Doctor and Clinic

        builder.Entity<Doctor>()
    .HasMany(d => d.Clinics)
    .WithMany(c => c.Doctors)
    .UsingEntity<Dictionary<string, object>>(
        "DoctorClinic", // The name of the junction table
        j => j.HasOne<Clinic>() 
            .WithMany()
            .HasForeignKey("ClinicId")
            .OnDelete(DeleteBehavior.NoAction), // can not delete Clinic if linked
        j => j.HasOne<Doctor>() 
            .WithMany()
            .HasForeignKey("DoctorId")
            .OnDelete(DeleteBehavior.Cascade) // Deleting a Doctor should CASCADE to delete the link
    );


    }
    public DbSet<Doctor> Doctors { get; set; }
    public DbSet<Patient> Patients { get; set; }
    public DbSet<Appointment> Appointments { get; set; }
    public DbSet<BillingRecord> BillingRecords { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Clinic> Clinics { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<DoctorSchedule> DoctorSchedules { get; set; }
    public DbSet<InsuranceProvider> InsuranceProviders { get; set; }
    public DbSet<Service> Services { get; set; }
}