using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.RazonSocial).HasMaxLength(300);
        builder.Property(x => x.Ruc).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Direccion).HasMaxLength(400);
        builder.Property(x => x.Ciudad).HasMaxLength(100);
        builder.Property(x => x.SitioWeb).HasMaxLength(300);
        builder.Property(x => x.Facebook).HasMaxLength(200);
        builder.Property(x => x.Instagram).HasMaxLength(200);
        builder.Property(x => x.WhatsApp).HasMaxLength(50);
    }
}
