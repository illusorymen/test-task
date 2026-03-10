using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using task.Domain.Entities;

namespace task.Infrastructure.Persistence;

public class PhoneConfiguration : IEntityTypeConfiguration<Phone>
{
    public void Configure(EntityTypeBuilder<Phone> builder)
    {
        builder.ToTable("phones");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.OfficeId).HasColumnName("office_id");
        builder
            .Property(e => e.PhoneNumber)
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnName("phone_number");
        builder.Property(e => e.Additional).HasMaxLength(200).HasColumnName("additional");

        // FK и подбор телефонов по офису
        builder.HasIndex(e => e.OfficeId).HasDatabaseName("ix_phones_office_id");
    }
}
