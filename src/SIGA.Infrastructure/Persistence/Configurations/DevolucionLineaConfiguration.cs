using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SIGA.Domain.Entities;

namespace SIGA.Infrastructure.Persistence.Configurations;

public class DevolucionLineaConfiguration : IEntityTypeConfiguration<DevolucionLinea>
{
    public void Configure(EntityTypeBuilder<DevolucionLinea> builder)
    {
        builder.ToTable("devolucion_lineas");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CantidadDevuelta).IsRequired();

        builder.HasOne(x => x.ProductoDevuelto)
            .WithMany()
            .HasForeignKey(x => x.ProductoDevueltoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ProductoNuevo)
            .WithMany()
            .HasForeignKey(x => x.ProductoNuevoId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
