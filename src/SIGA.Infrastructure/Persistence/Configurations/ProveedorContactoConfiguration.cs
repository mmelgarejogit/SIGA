using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class ProveedorContactoConfiguration : IEntityTypeConfiguration<ProveedorContacto>
{
    public void Configure(EntityTypeBuilder<ProveedorContacto> builder)
    {
        builder.ToTable("proveedor_contactos");

        builder.Property(x => x.Nombre).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Cargo).HasMaxLength(100);
        builder.Property(x => x.Telefono).HasMaxLength(50);
        builder.Property(x => x.Email).HasMaxLength(200);

        builder.HasOne(x => x.Proveedor)
               .WithMany(x => x.Contactos)
               .HasForeignKey(x => x.ProveedorId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
