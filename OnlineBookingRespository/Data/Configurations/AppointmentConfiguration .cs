using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookingCore.Entities;

namespace OnlineBookingRespository.Data.Configurations
{
    public class AppointmentConfiguration  : IEntityTypeConfiguration<Appointment>
    {
        public void Configure(EntityTypeBuilder<Appointment> builder)
        {
            builder.Property(a => a.BookedDurationMinutes).HasColumnType("decimal(18,2)");



            // Configure the one-to-many relationship between Appointment and Doctor
            builder.HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent deletion of Doctor if linked

            // Configure the one-to-many relationship between Appointment and Patient
            builder.HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.NoAction); // Prevent deletion of Patient if linked

            // Configure the one-to-one relationship between Appointment and BillingRecord
            builder.HasOne(a => a.BillingRecord)
                .WithOne(b => b.Appointment)
                .HasForeignKey<BillingRecord>(b => b.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade); // Deleting an Appointment should CASCADE to delete the BillingRecord

            // Configure the one-to-many relationship between Appointment and DoctorSchedule
         builder.HasOne(a => a.DoctorSchedule)
            .WithMany(ds => ds.Appointments)
            .HasForeignKey(a => a.DoctorScheduleId)
            .OnDelete(DeleteBehavior.NoAction); // Prevent deletion of DoctorSchedule if linked
        }

    }
}