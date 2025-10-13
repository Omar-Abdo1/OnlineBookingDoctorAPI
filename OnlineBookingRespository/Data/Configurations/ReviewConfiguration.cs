using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookingCore.Entities;

namespace OnlineBookingRespository.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {

            builder.HasOne(r => r.Patient)
            .WithMany(p => p.Reviews)
            .HasForeignKey(r => r.PatientId)
            .IsRequired(false) // Must be nullable to allow SET NULL
            .OnDelete(DeleteBehavior.SetNull); // SET NULL when Patient is deleted  to keep review history

        builder.HasOne(r => r.Doctor)
            .WithMany(d => d.Reviews)
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.NoAction); // can not delete Doctor if linked
        }    
        
    }
}