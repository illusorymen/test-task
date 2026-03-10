using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using task.Domain.Entities;

namespace task.Infrastructure.Persistence;

public class OfficeConfiguration : IEntityTypeConfiguration<Office>
{
    public void Configure(EntityTypeBuilder<Office> builder)
    {
        builder.ToTable("offices");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).HasColumnName("id");

        builder.Property(e => e.Code).HasMaxLength(50).HasColumnName("code");
        builder.Property(e => e.CityCode).HasColumnName("city_code");
        builder.Property(e => e.Uuid).HasMaxLength(100).HasColumnName("uuid");
        builder.Property(e => e.Type).HasColumnName("type");
        builder
            .Property(e => e.CountryCode)
            .HasMaxLength(10)
            .IsRequired()
            .HasColumnName("country_code");
        builder.Property(e => e.AddressRegion).HasMaxLength(500).HasColumnName("address_region");
        builder.Property(e => e.AddressCity).HasMaxLength(200).HasColumnName("address_city");
        builder.Property(e => e.AddressStreet).HasMaxLength(500).HasColumnName("address_street");
        builder
            .Property(e => e.AddressHouseNumber)
            .HasMaxLength(50)
            .HasColumnName("address_house_number");
        builder.Property(e => e.AddressApartment).HasColumnName("address_apartment");
        builder.Property(e => e.WorkTime).HasMaxLength(1000).HasColumnName("work_time");

        builder.OwnsOne(
            e => e.Coordinates,
            c =>
            {
                c.Property(x => x.Latitude).HasColumnName("latitude");
                c.Property(x => x.Longitude).HasColumnName("longitude");
            }
        );

        builder
            .HasMany(e => e.Phones)
            .WithOne(p => p.Office)
            .HasForeignKey(p => p.OfficeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Поиск по идентификатору города
        builder.HasIndex(e => e.CityCode).HasDatabaseName("ix_offices_city_code");
        // Поиск по названию города
        builder.HasIndex(e => e.AddressCity).HasDatabaseName("ix_offices_address_city");
        // Поиск по коду терминала
        builder.HasIndex(e => e.Code).HasDatabaseName("ix_offices_code");
        // Фильтрация по типу (ПВЗ, склад, постамат)
        builder.HasIndex(e => e.Type).HasDatabaseName("ix_offices_type");
        // Составной: город + тип — типичный сценарий «офисы в городе X типа Y»
        builder
            .HasIndex(e => new { e.CityCode, e.Type })
            .HasDatabaseName("ix_offices_city_code_type");
    }
}
