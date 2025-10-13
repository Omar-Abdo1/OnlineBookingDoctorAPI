using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookingCore.Entities;

namespace OnlineBookingRespository.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    
    void IEntityTypeConfiguration<Service>.Configure(EntityTypeBuilder<Service> builder)
    {
       builder.Property(s=>s.Price).HasColumnType("decimal(18,2)");
       builder.Property(s=>s.DurationInMinutes).HasColumnType("decimal(18,2)");
    }
}
