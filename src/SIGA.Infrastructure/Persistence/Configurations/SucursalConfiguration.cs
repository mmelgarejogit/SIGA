using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class SucursalConfiguration : IEntityTypeConfiguration<Sucursal>
{
    public void Configure(EntityTypeBuilder<Sucursal> builder)
    {
        builder.ToTable("sucursales");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Codigo).IsRequired().HasMaxLength(30);
        builder.HasIndex(x => x.Codigo).IsUnique();

        builder.Property(x => x.Direccion).HasMaxLength(250);
        builder.Property(x => x.Telefono).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(150);
        builder.Property(x => x.Establecimiento).HasMaxLength(10);

        builder.HasOne(x => x.Ciudad)
            .WithMany()
            .HasForeignKey(x => x.CiudadId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
