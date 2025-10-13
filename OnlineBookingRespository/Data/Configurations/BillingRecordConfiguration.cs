using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineBookingCore.Entities;

namespace OnlineBookingRespository.Data.Configurations;

public class BillingRecordConfiguration : IEntityTypeConfiguration<BillingRecord>
{
    public void Configure(EntityTypeBuilder<BillingRecord> builder)
    {
        builder.Property(b => b.Amount).
        HasColumnType("decimal(18,2)");
    }
}
