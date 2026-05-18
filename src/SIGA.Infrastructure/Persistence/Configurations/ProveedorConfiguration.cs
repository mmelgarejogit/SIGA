using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
{
    public void Configure(EntityTypeBuilder<Proveedor> builder)
    {
        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Contacto).HasMaxLength(200);
        builder.Property(x => x.Email).HasMaxLength(200);
        builder.Property(x => x.Telefono).HasMaxLength(50);
        builder.Property(x => x.Ruc).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Timbrado).IsRequired().HasMaxLength(8);
        builder.Property(x => x.VigenciaTimbrado);
        builder.Property(x => x.Establecimiento).HasMaxLength(7);
    }
}
